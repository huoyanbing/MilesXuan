using FPT.CommonClass;
using HansLinkManage.CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HansLinkManage
{
    
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            _mainForm = this;
        }

        public static MainForm _mainForm = null;
        static int currentCol = -1;
        private NamedPipe namedPipe;
        public  string masonPath;
        public string WaitPath;
        public string TestPath;
        public string PassPath;
        public string OpenPath;
        public string ClearPath;
        FileOperate fileOperate = new FileOperate();
        public BatchForm batchForm;
        public static SerialPortComm serialPortComm;
        public bool threadState;
        public static bool blpipe;
        public static bool blmanual;
        public static bool blresult;
        private delegate void ListViewHandler();
        LogHelper logHelper;
        bool flag = false;



        private void MasonMenuItem_Click(object sender, EventArgs e)
        {
            FolderForm folderForm = new FolderForm();
            folderForm.ShowDialog();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            threadState = false;
            blpipe = false;
            blmanual = false;
            blresult = false;
            serialPortComm = new SerialPortComm();
            namedPipe = new NamedPipe();
            namedPipe.ReadFPTEvent += ReadFptData;
            namedPipe.ShowPipeLEDOnvent += MainPictureON;
            namedPipe.ShowPipeLEDOffvent += MainPictureOFF;
            ThreadPool.QueueUserWorkItem(new WaitCallback(namedPipe.ConnectPipeThread));
            listView3.ListViewItemSorter = new ListViewColumnSorter();
            listView4.ListViewItemSorter = new ListViewColumnSorter();
            listView5.ListViewItemSorter = new ListViewColumnSorter();
            (listView4.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Descending;
            (listView5.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Descending;
            // listView3.ColumnClick += new ColumnClickEventHandler(ListView_ColumnClick);
            OpenSerialPortM();
            ThreadPool.QueueUserWorkItem(new WaitCallback(threadRecv));
            logHelper = new LogHelper("HLMLog");
            //fileOperate.ExecuteBatlFile();
            ttsPassBoard.Alignment = ToolStripItemAlignment.Right;
            ttsOpenBoard.Alignment = ToolStripItemAlignment.Right;
            tssRestBoard.Alignment = ToolStripItemAlignment.Right;
            tssTime.Alignment = ToolStripItemAlignment.Right;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            FolderForm folderForm = new FolderForm();
            folderForm.ShowDialog();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("主窗体将被关闭，是否继续？", "询问", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {                           
                    if (serialPortComm.WriteMultiSinglePoint("%01#WCSR01000**\r") == 1)
                       MessageBox.Show("R100=>0 Fail");                
               /* 
                string recvDataOne = "";
                if (serialPortComm.ReadMultiSinglePoint("%01#RCP1R0100**\r", 1, ref recvDataOne) == 0)
                {
                    char[] chsPLC = recvDataOne.ToCharArray();
                    if (chsPLC[0].Equals('0'))
                      { tssTipMessage.Text = "R100=>0 Succeed"; }
                }
                else
                {
                    MessageBox.Show("Read R100 Fail");
                }
                */
                if (batchForm==null)
                {
                    e.Cancel = false;
                }
                else
                {
                    batchForm.ReadOpenErpEvent -= new ReadOpenErpHandler(UpdateOpenListView);
                    while (fileOperate.blListViewRun)
                    {
                        Application.DoEvents();
                    }
                    batchForm.MyTimer.Stop();
                    batchForm.MyTimer.Close();
                    e.Cancel = false;
                }
                
            }
            else 
            {             
                e.Cancel = true;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        public void UpdateOpenListView()
        {
            this.Invoke(fileOperate.myListViewDelegate, masonPath, imageList1, listView3);
            this.Invoke(new ListViewHandler(() => { tssRestBoard.Text = " 剩余板数："+listView3.Items.Count.ToString(); }));
        }

        public void ReadFptData(string str)
        {
            if (blpipe)
            {
                if (str.CompareTo("START") == 0)
                {
                    int i;
                    for (i = 0; i < 10; i++)
                    {
                        if (serialPortComm.WriteMultiSinglePoint("%01#WCSR01001**\r") == 0)
                            break;
                    }
                    if (i == 10)
                    {
                        tssTipMessage.Text = "R100=>1 fail";
                        logHelper.EncWrite("R100=>1 fail");
                        MessageBox.Show("R100=>1 fail ！！！");
                    }
                    else
                        logHelper.EncWrite("R100=>1 Succssed");
                }
                if (str.CompareTo("OVER") == 0)
                {
                    int i;
                    for (i = 0; i < 10; i++)
                    {
                        if (serialPortComm.WriteMultiSinglePoint("%01#WCSR01000**\r") == 0)
                            break;
                    }
                    if (i == 10)
                    {                       
                        this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "R100=>0 fail"; }));
                        logHelper.EncWrite("R100=>0 fail");
                        MessageBox.Show("R100=>0 fail ！！！");
                    }
                    else
                        logHelper.EncWrite("R100=>0 Succssed");
                    for (int j = 0; j < 100; j++)
                    {
                        if (namedPipe.OnPipeWrite("OVER") == 0)
                            break;
                        else
                        {
                            if (MessageBox.Show("飞针软件被关闭或通讯断开，是否重新连接？", "询问", MessageBoxButtons.YesNo) == DialogResult.No)
                            {
                                threadState = false;
                                this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "OVER发送失败！！！"; }));
                                MessageBox.Show("OVER发送失败！！！");
                                break;
                            }

                        }
                    }

                }
                if (str.CompareTo("TESTOK") == 0)
                {
                    if (blresult)
                    {
                        if (MessageBox.Show("飞针软件是否测试完成？", "询问", MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            MessageBox.Show("通讯流程出现错误，请清除PLC信号重新开始！！！");
                            return;
                        }
                        else
                        {
                            if (MessageBox.Show("飞针软件是否回零完成？", "询问", MessageBoxButtons.YesNo) == DialogResult.No)
                            {
                                MessageBox.Show("通讯流程出现错误，请清除PLC信号重新开始！！！");
                                return;
                            }
                            Thread.Sleep(1000);
                        }
                    }
                    blresult = false;
                    int i;
                    for ( i=0;i<10;i++)
                    {
                        if (0 == serialPortComm.WriteMultiSinglePoint("%01#WCP4R01011R01020R01030R01060**\r"))
                            break;
                    }
                    if(i==10)
                    {
                        this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Send TESTOK to PLC Fail"; }));
                        MessageBox.Show("Send TESTOK to PLC Fail");
                        logHelper.EncWrite("Send TESTOK to PLC Fail");
                    }
                    logHelper.EncWrite("⑧TESTOK");
                }
                if (str.CompareTo("TESTNG") == 0)
                {
                    int i;
                    for (i = 0; i < 10; i++)
                    {
                        if (0 == serialPortComm.WriteMultiSinglePoint("%01#WCP4R01010R01021R01030R01060**\r"))
                            break;
                    }
                    if (i == 10)
                    {
                        this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Send TESTNG to PLC Fail"; }));
                        MessageBox.Show("Send TESTNG to PLC Fail");
                        logHelper.EncWrite("Send TESTNG to PLC Fail");
                    }
                    logHelper.EncWrite("⑧TESTNG");
                }
                if (str.CompareTo("TESTLEAK") == 0)
                {
                    int i;
                    for (i = 0; i < 10; i++)
                    {
                        if (0 == serialPortComm.WriteMultiSinglePoint("%01#WCP4R01011R01020R01030R01061**\r"))
                            break;
                    }
                    if (i == 10)
                    {
                        this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Send TESTLEAK to PLC Fail"; }));
                        MessageBox.Show("Send TESTLEAK to PLC Fail");
                        logHelper.EncWrite("Send TESTLEAK to PLC Fail");
                    }
                    
                }
            }
        }

        public void GetWTListView()
        {
            fileOperate.GetListViewItemFileWT(WaitPath, imageList1, listView1);
            fileOperate.GetListViewItemFileWT(TestPath, imageList1, listView2);
            fileOperate.GetListViewItemFileWT(PassPath, imageList1, listView4);
            fileOperate.GetListViewItemFileWT(OpenPath, imageList1, listView5);
            if (listView1.Items.Count>1)
            {
                MessageBox.Show("等待位erp文件数有误！！！");
                logHelper.EncWrite("等待位erp文件数有误！！！");
            }
            if (listView2.Items.Count > 1)
            {
                MessageBox.Show("测试位erp文件数有误！！！");
                logHelper.EncWrite("测试位erp文件数有误！！！");
            }
            threadState = true;
            blpipe = true;           
            ttsPassBoard.Text = "    PASS板数：" + fileOperate.GetFileCount(PassPath, ".erp").ToString();
            ttsOpenBoard.Text = "    OPEN板数：" + fileOperate.GetFileCount(OpenPath, ".erp").ToString();
        }

        //public void StartMotionThread()
        //{
        //    ThreadPool.QueueUserWorkItem(new WaitCallback(OnReadFPTThread));
        //}

        //public void OnReadFPTThread(object state)
        //{
            
        //}



        public static void ListView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            string Asc = ((char)0x25bc).ToString().PadLeft(2, ' ');
            string Des = ((char)0x25b2).ToString().PadLeft(2, ' ');
            System.Windows.Forms.ListView lv = sender as System.Windows.Forms.ListView;
            // 检查点击的列是不是现在的排序列.
            if (e.Column == (lv.ListViewItemSorter as ListViewColumnSorter).SortColumn)
            {
                // 重新设置此列的排序方法.
                if ((lv.ListViewItemSorter as ListViewColumnSorter).Order == System.Windows.Forms.SortOrder.Ascending)
                {
                    (lv.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Descending;
                    string oldStr = lv.Columns[e.Column].Text.TrimEnd((char)0x25bc, (char)0x25b2, ' ');
                    lv.Columns[e.Column].Text = oldStr + Asc;
                }
                else
                {
                    (lv.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Ascending;
                    string oldStr = lv.Columns[e.Column].Text.TrimEnd((char)0x25bc, (char)0x25b2, ' ');
                    lv.Columns[e.Column].Text = oldStr + Des;
                }
            }
            else
            {
                // 设置排序列，默认为正向排序
                (lv.ListViewItemSorter as ListViewColumnSorter).SortColumn = e.Column;
                (lv.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Ascending;
                string oldStr = lv.Columns[e.Column].Text.TrimEnd((char)0x25bc, (char)0x25b2, ' ');
                lv.Columns[e.Column].Text = oldStr + Asc;
            }
           // 用新的排序方法对ListView排序
           ((System.Windows.Forms.ListView)sender).Sort();
            int rowCount = lv.Items.Count;
            if (currentCol != -1)
            {
                 if (e.Column != currentCol)
                    lv.Columns[currentCol].Text = lv.Columns[currentCol].Text.TrimEnd((char)0x25bc, (char)0x25b2, ' ');
            }
            currentCol = e.Column;
        }

        private void ComToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (serialPortComm.IsOpen)
            {
                threadState = false;
                serialPortComm.CloseSerialPort();
                tssTipMessage.Text = "串口关闭";
                pictureBox1.Image = (Image)Properties.Resources.ResourceManager.GetObject("OFF");
            }
            SerialPortForm serialPortForm = new SerialPortForm();
            serialPortForm.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tssTime.Text ="    "+ DateTime.Now.ToString();
        }

        public string MaintextBox1
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public string MaintextBox2
        {
            get { return textBox2.Text; }
            set { textBox2.Text = value; }
        }

        public void MainPictureON()
        {
            pictureBox2.Image = (Image)Properties.Resources.ResourceManager.GetObject("ON");
        }

        public void MainPictureOFF()
        {
            pictureBox2.Image = (Image)Properties.Resources.ResourceManager.GetObject("OFF");
        }

        private int InitCom()
        {
            
            try
            { serialPortComm.PortName = Properties.Settings.Default.PortName; }
            catch
            { serialPortComm.PortName = "COM1"; }
            serialPortComm.BaudRate = fileOperate.ToInt32(Properties.Settings.Default.BaudRate,115200);
            serialPortComm.DataBits = fileOperate.ToInt32(Properties.Settings.Default.DataBits,8);
            try
            { serialPortComm.StopBits = (StopBits)Enum.Parse(typeof(StopBits), Properties.Settings.Default.StopBits); }
            catch
            { serialPortComm.StopBits = StopBits.One; }
            try
            { serialPortComm.Parity = (Parity)Enum.Parse(typeof(Parity), Properties.Settings.Default.Parity); }
            catch
            { serialPortComm.Parity = Parity.Odd; }

            try
            {
                serialPortComm.SetSerialPort();
            }
            catch
            {
                return 1;
            }
            return 0;
        }

        public void OpenSerialPortM()
        {
            if (InitCom() == 1)
            {
                MessageBox.Show("无效串口！", "提示", MessageBoxButtons.OK);
                return;
            }
            if (1 == serialPortComm.OpenSerialPort())
            {
                MessageBox.Show("打开串口失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                tssTipMessage.Text = "串口打开";
                pictureBox1.Image = (Image)Properties.Resources.ResourceManager.GetObject("ON");

                // btnOpenPort.Text = "关闭串口";
                // SerialPortComm.SetStatusOpened();
            }
        }

        public void threadRecv(object state)
        {
            //int a = 0;
            string strPLC = "%01#RCP7R0100R0101R0102R0103R0104R0105R0106**\r";
            while (true)
            {
                // if(a<10)
                //  {
                //      logHelper.EncWrite("Send TESTLEAK to PLC Fail");
                //      return;            
                //  }

                if (threadState)
                {
                    this.Invoke(new ListViewHandler(() => { pictureBox3.Image = (Image)Properties.Resources.ResourceManager.GetObject("ON"); }));
                    string recvDataPLC = "";
                    if (serialPortComm.ReadMultiSinglePoint(strPLC, 7, ref recvDataPLC) == 0)
                    {
                        char[] chsPLC = recvDataPLC.ToCharArray();
                        if (chsPLC[0].Equals('1'))
                        { }
                        if (chsPLC[1].Equals('1'))
                        {
                            logHelper.EncWrite("⑨R101=>1");
                            string fileOK = "";
                            this.Invoke(new ListViewHandler(() => {
                                if(listView2.Items.Count==0)
                                {
                                    this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                    MessageBox.Show("测试位无erp资料");
                                    return;
                                }
                                fileOK = listView2.Items[0].SubItems[0].Text;
                            }));
                            if (string.IsNullOrEmpty(fileOK))
                            {                               
                                this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                logHelper.EncWrite("TestErp is Empty ");
                                MessageBox.Show("TestErp is Empty ");
                                return;
                            }
                            fileOperate.Move(TestPath + "\\" + fileOK, PassPath);
                            this.Invoke(fileOperate.myListViewWTDelegate, TestPath, imageList1, listView2);
                            this.Invoke(fileOperate.myListViewOneDelegate, PassPath+ "\\" + fileOK, imageList1, listView4);
                            logHelper.EncWrite("⑩PASS位： " + fileOK);
                            //Thread.Sleep(100);
                            this.Invoke(new ListViewHandler(() => { ttsPassBoard.Text = "    PASS板数：" + fileOperate.GetFileCount(PassPath, ".erp").ToString(); }));
                            int i;
                            for (i = 0; i < 5; i++)
                            {
                                if (0 == serialPortComm.WriteMultiSinglePoint("%01#WCSR01010**\r"))
                                    break;
                            }
                            if (i == 5)
                            {
                                this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                logHelper.EncWrite("R101=>0 fail");
                                MessageBox.Show("R101=>0 fail");
                                return;
                            }
                            logHelper.EncWrite("⑪R101=>0");
                        }
                        if (chsPLC[2].Equals('1'))
                        {
                            logHelper.EncWrite("⑨R102=>1");
                            string fileNG = "";
                            this.Invoke(new ListViewHandler(() => {
                                if (listView2.Items.Count == 0)
                                {
                                    this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                    MessageBox.Show("测试位无erp资料");                              
                                    return;
                                }
                                fileNG = listView2.Items[0].SubItems[0].Text; }));
                            if (string.IsNullOrEmpty(fileNG))
                            {                                
                                this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                logHelper.EncWrite("TestErp is Empty ");
                                MessageBox.Show("TestErp is Empty ");
                                return;
                            }
                            fileOperate.Move(TestPath + "\\" + fileNG, OpenPath);
                            this.Invoke(fileOperate.myListViewWTDelegate, TestPath, imageList1, listView2);
                            this.Invoke(fileOperate.myListViewOneDelegate, OpenPath + "\\" + fileNG, imageList1, listView5);
                            logHelper.EncWrite("⑩OPEN位： " + fileNG);
                            // Thread.Sleep(100);
                            this.Invoke(new ListViewHandler(() => { ttsOpenBoard.Text = "    OPEN板数：" + fileOperate.GetFileCount(OpenPath, ".erp").ToString(); }));
                            int i;
                            for (i = 0; i < 5; i++)
                            {
                                if (0 == serialPortComm.WriteMultiSinglePoint("%01#WCSR01020**\r"))
                                    break;
                            }
                            if (i == 5)
                            {
                                this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                logHelper.EncWrite("R102=>0 fail");
                                MessageBox.Show("R102=>0 fail");
                                return;
                            }
                            logHelper.EncWrite("⑪R102=>0");
                        }
                        if (chsPLC[5].Equals('1'))
                        {
                            logHelper.EncWrite("④R105=>1");                           
                        waitnext:
                            string fileTst = "";
                            this.Invoke(new ListViewHandler(() => {
                                if (listView1.Items.Count == 0)
                                {
                                    this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                    logHelper.EncWrite("等待位无erp资料");
                                    MessageBox.Show("等待位无erp资料");
                                    return;
                                }
                                fileTst = listView1.Items[0].SubItems[0].Text;
                            }));
                            if (string.IsNullOrEmpty(fileTst))
                            {
                                this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                logHelper.EncWrite("WaitErp is Empty ");
                                MessageBox.Show("WaitErp is Empty ");
                                return;
                            }
                            if (fileOperate.IsExistFile(WaitPath + "\\" + fileTst))
                                fileOperate.Move(WaitPath + "\\" + fileTst, TestPath);
                            else
                            {
                                goto waitnext;
                            }
                            logHelper.EncWrite("⑤TestFile："+ fileTst);
                            this.Invoke(fileOperate.myListViewWTDelegate, WaitPath, imageList1, listView1);
                            this.Invoke(fileOperate.myListViewWTDelegate, TestPath, imageList1, listView2);
                            if (listView2.Items.Count > 1)
                            {
                                if (MessageBox.Show("测试位erp文件次序错误，测试位上一片PCB是否被人工取走？", "TestErp", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    if (MessageBox.Show("是否删除上一片PCB的erp文件？", "TestErp", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    {
                                        if (listView2.Items.Count == 0)
                                            return;
                                        string TstFile = listView2.Items[0].SubItems[0].Text;
                                        if (string.IsNullOrEmpty(TstFile))
                                        {
                                            this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "NO TestErp file Error"; }));                                     
                                            MessageBox.Show("测试位没有erp文件");
                                            return;
                                        }
                                        fileOperate.Move(TestPath + "\\" + TstFile, ClearPath);
                                        this.Invoke(fileOperate.myListViewWTDelegate, TestPath, imageList1, listView2);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("测试位erp文件次序错误,请停机查看！！！ ");
                                    if (serialPortComm.WriteMultiSinglePoint("%01#WCSL016E1**\r") == 1)
                                    {
                                        this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "L16E=>1 fail"; }));
                                        logHelper.EncWrite("L16E=>1 fail");
                                        MessageBox.Show("L16E=>1 fail");
                                    }
                                    else
                                    {
                                        this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                        logHelper.EncWrite("erp文件次序错误 ");
                                        return;
                                    }
                                }
                            }
                            for (int j = 0; j < 100; j++)
                            {
                                if (namedPipe.OnPipeWrite(TestPath + "\\" + fileTst) == 0)
                                    break;
                                else
                                {
                                    if (MessageBox.Show("飞针软件被关闭或通讯断开，是否重新连接？", "询问", MessageBoxButtons.YesNo) == DialogResult.No)
                                    {
                                        threadState = false;
                                        MessageBox.Show("请清除测试位资料或手动发送资料给飞针软件");
                                        break;
                                    }

                                }
                            }
                            logHelper.EncWrite("⑥PipeSendFile：" + fileTst);
                            int i;
                            for (i = 0; i < 5; i++)
                            {
                                if (0 == serialPortComm.WriteMultiSinglePoint("%01#WCSR01050**\r"))
                                    break;
                            }
                            if (i == 5)
                            {
                                this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                logHelper.EncWrite("R105=>0 fail");
                                MessageBox.Show("R105=>0 fail");
                                return;
                            }
                            logHelper.EncWrite("⑦R105=>0");
                        }

                        if (chsPLC[4].Equals('1'))
                        {
                            logHelper.EncWrite("①R104=>1");                           
                        masonnext:
                            string fileWait = "";
                            this.Invoke(new ListViewHandler(() => {
                                if (listView3.Items.Count == 0)
                                {
                                    this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                    logHelper.EncWrite("Thread is Jump ");
                                    MessageBox.Show("Mason文件夹无erp资料");
                                    return;
                                }
                                fileWait = listView3.Items[0].SubItems[0].Text; }));
                            if (string.IsNullOrEmpty(fileWait))
                            {
                                this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));                               
                                logHelper.EncWrite("MasonErp is Empty ");
                                MessageBox.Show("MasonErp is Empty ");
                                return;
                            }
                            if (fileOperate.IsExistFile(masonPath + "\\" + fileWait))
                                fileOperate.Move(masonPath + "\\" + fileWait, WaitPath);
                            else
                            {
                                goto masonnext;
                            }
                            logHelper.EncWrite("②WaitFile："+ fileWait);
                            this.Invoke(fileOperate.myListViewWTDelegate, WaitPath, imageList1, listView1);
                            
                            if (listView1.Items.Count >1)
                            {
                                logHelper.EncWrite("等待位erp文件测序错误!!!");
                                
                                
                                if (MessageBox.Show("等待位erp文件次序错误，等待位上一片PCB是否被人工取走？", "WaitErp", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                        if (MessageBox.Show("是否删除上一片PCB的erp文件？", "WaitErp", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        {
                                            if (listView1.Items.Count == 0)
                                                return;
                                            string WaitTst = listView1.Items[0].SubItems[0].Text;
                                            if (string.IsNullOrEmpty(WaitTst))
                                            {
                                                this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "NO TestErp file Error"; }));
                                                MessageBox.Show("等待位没有erp文件");
                                                return;
                                            }
                                            fileOperate.Move(WaitPath + "\\" + WaitTst, ClearPath);
                                            this.Invoke(fileOperate.myListViewWTDelegate, WaitPath, imageList1, listView1);
                                        }
                                }                              
                                else
                                {
                                    MessageBox.Show("等待位erp文件次序错误,请停机查看！！！ ");
                                    if (serialPortComm.WriteMultiSinglePoint("%01#WCSL016E1**\r") == 1)
                                    {
                                        this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "L16E=>1 fail"; }));
                                        logHelper.EncWrite("L16E=>1 fail");
                                        MessageBox.Show("L16E=>1 fail");                                        
                                    }
                                    else
                                    {
                                        this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                        logHelper.EncWrite("erp文件次序错误 ");
                                        return;
                                    }
                                }
                            }
                        R104next:
                            int i;
                            for (i = 0; i < 5; i++)
                            {
                                if (0 == serialPortComm.WriteMultiSinglePoint("%01#WCSR01040**\r"))
                                    break;
                            }
                            if (i == 5)
                            {
                                this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                logHelper.EncWrite("R104=>0 fail");
                                MessageBox.Show("R104=>0 fail");
                                return;
                            }
                            string recvDataOne = "";
                            if (serialPortComm.ReadMultiSinglePoint("%01#RCP1R0104**\r", 1, ref recvDataOne) == 0)
                            {
                                char[] chsPL = recvDataOne.ToCharArray();
                                if (chsPL[0].Equals('0'))
                                    logHelper.EncWrite("③R104=>0");
                                else
                                    goto R104next;
                            }
                            else
                            {
                                goto R104next;
                            }

                            if (blmanual)
                            {
                                if (fileOperate.GetFileCount(masonPath, ".erp") == 0)
                                {
                                    if (serialPortComm.WriteMultiSinglePoint("%01#WCSL016F1**\r") == 1)
                                    {
                                        this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "Thread is Jump"; }));
                                        logHelper.EncWrite("L16F=>1 fail");
                                        MessageBox.Show("L16F=>1 fail");
                                        return;
                                    }
                                    else
                                    {
                                        this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "测试完成"; }));
                                        logHelper.EncWrite("测试完成");
                                       // return;
                                    }
                                }

                            }

                        }
                       
                        if (chsPLC[6].Equals('1'))
                        {
                            if (serialPortComm.WriteMultiSinglePoint("%01#WCSR01060**\r") == 1)
                            {
                                this.Invoke(new ListViewHandler(() => { tssTipMessage.Text = "R106=>0 fail"; }));
                                logHelper.EncWrite("R106=>0 fail");
                                MessageBox.Show("R106=>0 fail");
                            }
                        }

                    }
                    else
                    {

                        if (!serialPortComm.IsOpen)
                        {
                            pictureBox1.Image = (Image)Properties.Resources.ResourceManager.GetObject("OFF");
                        }
                    }
                }
                //丢弃输入缓冲区
                serialPortComm.DiscardInBuffer();
                //线程睡眠
                Thread.Sleep(100);
                this.Invoke(new ListViewHandler(() => { pictureBox3.Image = (Image)Properties.Resources.ResourceManager.GetObject("OFF"); }));
                Thread.Sleep(100);
            }
        }

        private void WaitErpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
                return;
            string  WaitTst = listView1.Items[0].SubItems[0].Text;
            if (string.IsNullOrEmpty(WaitTst))
            {
                tssTipMessage.Text = "NO WaitErp file Error";
                MessageBox.Show("等待位没有erp文件");
                return;
            }
            fileOperate.Move(WaitPath + "\\" + WaitTst, ClearPath);
            fileOperate.GetListViewItemFileWT(WaitPath,imageList1,listView1);
        }

        private void TstErpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.Items.Count == 0)
                return;
            string TstFile = listView2.Items[0].SubItems[0].Text;
            if (string.IsNullOrEmpty(TstFile))
            {
                tssTipMessage.Text = "NO TestErp file Error";
                MessageBox.Show("测试位没有erp文件");
                return;
            }
            fileOperate.Move(TestPath + "\\" + TstFile, ClearPath);
            fileOperate.GetListViewItemFileWT(TestPath, imageList1, listView2);
        }

        private void tssTipMessage_TextChanged(object sender, EventArgs e)
        {
            if (tssTipMessage.Text == "就绪"|| tssTipMessage.Text == "串口打开")
            {
                return;
            }
            else
            {
                tssTipMessage.ForeColor = Color.Red;
                timer2.Start();
                timer2.Interval = 120000;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            tssTipMessage.Text = "就绪";
            tssTipMessage.ForeColor = Color.Black;
            timer2.Stop();
        }

        private void RecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!flag)
            {
                textBoxRecord.Visible = true;
                textBoxRecord.BringToFront();
                StreamReader streamReader = null;
                try
                {
                    StringBuilder sb = new StringBuilder("");
                    string strtime = DateTime.Now.ToString("yyyy-MM-dd");
                    string logpath = Application.StartupPath + "\\" + "Log" + "\\" + strtime + "\\" + "HLMLog.log";
                    if (File.Exists(logpath))
                    {
                        streamReader = new StreamReader(logpath, Encoding.UTF8);
                       // string str123 = streamReader.ReadLine();
                        string line = fileOperate.ToDecrypt("9685", streamReader.ReadLine());                      
                        while (!string.IsNullOrEmpty(line))
                        {
                            sb.Append(line + "\n");
                            line = fileOperate.ToDecrypt("9685", streamReader.ReadLine());
                            // line = streamReader.ReadLine();
                        }
                        textBoxRecord.Text = sb.ToString();
                    }
                    else
                    {
                        MessageBox.Show("无Log文件");
                        textBoxRecord.Visible = false;
                    }

                }
                catch (Exception ee)
                {
                    MessageBox.Show("" + ee.Message);
                }
                finally
                {
                    if (streamReader != null)
                    {
                        streamReader.Close();
                    }
                }
        }
            else
            {
                textBoxRecord.Visible = false;
            }
            flag = !flag;
        }

        private void listView2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right&& listView2.SelectedItems!=null)
            {
                if (listView2.SelectedIndices.Count > 0)
                {
                    var hitTestInfo = listView2.HitTest(e.X, e.Y);
                    if (hitTestInfo.Item != null)
                    {
                        var loc = e.Location;
                        loc.Offset(listView2.Location);
                        this.contextMenuStrip1.Show(this, loc);
                    }
                }
                
            }
        }

        private void TSMenuItemTstErp_Click(object sender, EventArgs e)
        {
            string strName = listView2.SelectedItems[0].Text.ToString();
            for (int i = 0; i < 100; i++)
            {
                if (namedPipe.OnPipeWrite(TestPath + "\\" + strName) == 0)
                    break;
                else
                {
                    if (MessageBox.Show("飞针软件被关闭或通讯断开，是否重新连接？", "询问", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        MessageBox.Show("发送测试位资料给飞针软件失败！！！");
                        break;
                    }

                }
            }
        }

        private void ClearPassTSMenuItem_Click(object sender, EventArgs e)
        {
            if(listView4.Items.Count>0)
            {
                DirectoryInfo DInfo = new DirectoryInfo(PassPath);                                    
                FileSystemInfo[] FSInfo = DInfo.GetFileSystemInfos();                                       
                for (int i = 0; i < FSInfo.Length; i++)                                                           
                {
                    FileInfo FInfo = new FileInfo(PassPath + "\\" + FSInfo[i].ToString());            
                    FInfo.Delete();                                                               
                }
            }
            fileOperate.GetListViewItemFileWT(PassPath, imageList1, listView4);
        }

        private void ClearOpenTSMenuItem_Click(object sender, EventArgs e)
        {
            if (listView5.Items.Count > 0)
            {
                DirectoryInfo DInfo = new DirectoryInfo(OpenPath);
                FileSystemInfo[] FSInfo = DInfo.GetFileSystemInfos();
                for (int i = 0; i < FSInfo.Length; i++)
                {
                    FileInfo FInfo = new FileInfo(OpenPath + "\\" + FSInfo[i].ToString());
                    FInfo.Delete();
                }
            }
            fileOperate.GetListViewItemFileWT(PassPath, imageList1, listView5);
        }

        private void ScanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileForm fileForm = new FileForm();
            fileForm.ShowDialog();
        }

        private void AscOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (listView3.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Ascending;
            listView3.Sort();
        }

        private void desOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (listView3.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Descending;
            listView3.Sort();
        }

        private void StartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i;
            for (i = 0; i < 10; i++)
            {
                if (serialPortComm.WriteMultiSinglePoint("%01#WCSR01001**\r") == 0)
                    break;
            }
            if (i == 10)
            {
                tssTipMessage.Text = "R100=>1 fail";
                logHelper.EncWrite("R100=>1 fail");
                MessageBox.Show("R100=>1 fail ！！！");
            }
            else
                logHelper.EncWrite("R100=>1 Succssed");
        }
    }

    public class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// 指定按照哪个列排序
        /// </summary>
        private int ColumnToSort;
        /// <summary>
        /// 指定排序的方式
        /// </summary>
        private System.Windows.Forms.SortOrder OrderOfSort;
        /// <summary>
        /// 声明CaseInsensitiveComparer类对象
        /// </summary>
        private System.Collections.CaseInsensitiveComparer ObjectCompare;
        /// <summary>
        /// 构造函数
        /// </summary>
        public ListViewColumnSorter()
        {
            // 默认按第一列排序
            ColumnToSort = 0;
            // 排序方式为不排序
            OrderOfSort = System.Windows.Forms.SortOrder.Ascending;
            // 初始化CaseInsensitiveComparer类对象
            ObjectCompare = new System.Collections.CaseInsensitiveComparer();
        }
        /// <summary>
        /// 重写IComparer接口.
        /// </summary>
        /// <param name="x">要比较的第一个对象</param>
        /// <param name="y">要比较的第二个对象</param>
        /// <returns>比较的结果.如果相等返回0，如果x大于y返回1，如果x小于y返回-1</returns>
        public int Compare(object x, object y)
        {
            int compareResult;
            System.Windows.Forms.ListViewItem listviewX, listviewY;
            // 将比较对象转换为ListViewItem对象
            listviewX = (System.Windows.Forms.ListViewItem)x;
            listviewY = (System.Windows.Forms.ListViewItem)y;
            string xText = listviewX.SubItems[ColumnToSort].Text;
            string yText = listviewY.SubItems[ColumnToSort].Text;
            if (Regex.Matches(xText,@"\.").Count==1)
            {
                xText = xText.Remove(xText.LastIndexOf('.'));
                yText = yText.Remove(yText.LastIndexOf('.'));
            }        
            int xInt, yInt;
            // 比较,如果值为IP地址，则根据IP地址的规则排序。
            if (IsIP(xText) && IsIP(yText))
            {
                compareResult = CompareIp(xText, yText);
            }
            else if (int.TryParse(xText, out xInt) && int.TryParse(yText, out yInt)) //是否全为数字
            {
                //比较数字
                compareResult = CompareInt(xInt, yInt);
            }
            else
            {
                //比较对象
                compareResult = ObjectCompare.Compare(xText, yText);
            }
            // 根据上面的比较结果返回正确的比较结果
            if (OrderOfSort == System.Windows.Forms.SortOrder.Ascending)
            {
                // 因为是正序排序，所以直接返回结果
                return compareResult;
            }
            else if (OrderOfSort == System.Windows.Forms.SortOrder.Descending)
            {
                // 如果是反序排序，所以要取负值再返回
                return (-compareResult);
            }
            else
            {
                // 如果相等返回0
                return 0;
            }
        }
        /// <summary>
        /// 判断是否为正确的IP地址，IP范围（0.0.0.0～255.255.255）
        /// </summary>
        /// <param name="ip">需验证的IP地址</param>
        /// <returns></returns>
        public bool IsIP(String ip)
        {
            return System.Text.RegularExpressions.Regex.Match(ip, @"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$").Success;
        }
        /// <summary>
        /// 比较两个数字的大小
        /// </summary>
        /// <param name="ipx">要比较的第一个对象</param>
        /// <param name="ipy">要比较的第二个对象</param>
        /// <returns>比较的结果.如果相等返回0，如果x大于y返回1，如果x小于y返回-1</returns>
        private int CompareInt(int x, int y)
        {
            if (x > y)
            {
                return 1;
            }
            else if (x < y)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 比较两个IP地址的大小
        /// </summary>
        /// <param name="ipx">要比较的第一个对象</param>
        /// <param name="ipy">要比较的第二个对象</param>
        /// <returns>比较的结果.如果相等返回0，如果x大于y返回1，如果x小于y返回-1</returns>
        private int CompareIp(string ipx, string ipy)
        {
            string[] ipxs = ipx.Split('.');
            string[] ipys = ipy.Split('.');
            for (int i = 0; i < 4; i++)
            {
                if (Convert.ToInt32(ipxs[i]) > Convert.ToInt32(ipys[i]))
                {
                    return 1;
                }
                else if (Convert.ToInt32(ipxs[i]) < Convert.ToInt32(ipys[i]))
                {
                    return -1;
                }
                else
                {
                    continue;
                }
            }
            return 0;
        }
        /// <summary>
        /// 获取或设置按照哪一列排序.
        /// </summary>
        public int SortColumn
        {
            set
            {
                ColumnToSort = value;
            }
            get
            {
                return ColumnToSort;
            }
        }
        /// <summary>
        /// 获取或设置排序方式.
        /// </summary>
        public System.Windows.Forms.SortOrder Order
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }
    }
}
