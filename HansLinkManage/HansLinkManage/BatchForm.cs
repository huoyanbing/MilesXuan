using FPT.CommonClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;

namespace HansLinkManage
{
    public delegate void ReadOpenErpHandler();
    public partial class BatchForm : Form
    {
        private string filepath;
     //   public static MainForm mForm;
        public event ReadOpenErpHandler ReadOpenErpEvent;

        //public BatchForm(MainForm formMain)
        //{
        //    mForm = formMain;
        //}
        public BatchForm()
        {
            InitializeComponent();
        }

        FileOperate bFileOperate = new FileOperate();
        public System.Timers.Timer MyTimer;

        private string m_folderName;
        public string mFolderName
        {
            get { return m_folderName; }
            set { m_folderName = value; }
        }

        private void BatchForm_Load(object sender, EventArgs e)
        {
            filepath = FileOperate.inipath + "\\" + m_folderName;
            bFileOperate.GetListViewItemDir(filepath, imageList1, listView1);
            ReadOpenErpEvent += new ReadOpenErpHandler(MainForm._mainForm.UpdateOpenListView);
            MyTimer = new System.Timers.Timer(2000);
            MyTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerRun);
        }

        private void BatchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();
            FolderForm folderForm = new FolderForm();
            folderForm.ShowDialog();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
            FolderForm folderForm = new FolderForm();
            folderForm.ShowDialog();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
                return;
            Hide();
            string file_path = filepath + "\\" + listView1.SelectedItems[0].Text;
            MainForm._mainForm.masonPath = file_path;
            MainForm._mainForm.MaintextBox2 = listView1.SelectedItems[0].Text;
            bFileOperate.AllPath = file_path;
            bFileOperate.CreateFileDir("Hans_Wait", 1);
            bFileOperate.CreateFileDir("Hans_Test", 1);
            bFileOperate.CreateFileDir("Hans_Open", 1);
            bFileOperate.CreateFileDir("Hans_Pass", 1);
            bFileOperate.CreateFileDir("Hans_Clear", 1);
            MainForm._mainForm.WaitPath = file_path + "\\" + "Hans_Wait";
            MainForm._mainForm.TestPath = file_path + "\\" + "Hans_Test";
            MainForm._mainForm.OpenPath = file_path + "\\" + "Hans_Open";
            MainForm._mainForm.PassPath = file_path + "\\" + "Hans_Pass";
            MainForm._mainForm.ClearPath = file_path + "\\" + "Hans_Clear";
            OnTimerRun(null, null);
            MyTimer.Start();
            MainForm._mainForm.GetWTListView();
            string striName = FileOperate.inipath;
            if (striName.Contains("HansFPT"))
                MainForm.blmanual = true;
            // mForm.GetBatchFormTimer(MyTimer);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            Hide();
            string file_path = filepath + "\\" + listView1.SelectedItems[0].Text;
            MainForm._mainForm.masonPath = file_path;
            MainForm._mainForm.MaintextBox2 = listView1.SelectedItems[0].Text;
            bFileOperate.AllPath = file_path;
            bFileOperate.CreateFileDir("Hans_Wait", 1);
            bFileOperate.CreateFileDir("Hans_Test", 1);
            bFileOperate.CreateFileDir("Hans_Open", 1);
            bFileOperate.CreateFileDir("Hans_Pass", 1);
            bFileOperate.CreateFileDir("Hans_Clear", 1);
            MainForm._mainForm.WaitPath = file_path + "\\" + "Hans_Wait";
            MainForm._mainForm.TestPath = file_path + "\\" + "Hans_Test";
            MainForm._mainForm.OpenPath = file_path + "\\" + "Hans_Open";
            MainForm._mainForm.PassPath = file_path + "\\" + "Hans_Pass";
            MainForm._mainForm.ClearPath = file_path + "\\" + "Hans_Clear";
            OnTimerRun(null, null);
            MyTimer.Start();
            MainForm._mainForm.GetWTListView();
            string striName = FileOperate.inipath;
            if (striName.Contains("HansFPT"))
                MainForm.blmanual = true;
            // mForm.GetBatchFormTimer(MyTimer);
        }

        private void OnTimerRun(object sourse,ElapsedEventArgs e)
        {
            if (ReadOpenErpEvent!=null)
            {
                ReadOpenErpEvent();
            }
        }
    }
}
