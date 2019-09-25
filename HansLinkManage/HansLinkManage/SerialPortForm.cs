using HansLinkManage.CommonClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HansLinkManage
{
    public partial class SerialPortForm : Form
    {
        public SerialPortForm()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            GetPortName();
            if (PortName_ComboBox.Text == "")
            {
                PortName_ComboBox.Text = "COM1";
            }
            if (DataBits_ComboBox.Text == "")
            {
                DataBits_ComboBox.Text = "8";
            }
            if (StopBits_ComboBox.Text == "")
            {
                StopBits_ComboBox.Text = "1";
            }
            if (Parity_ComboBox.Text == "")
            {
                Parity_ComboBox.Text = "None";
            }
        }

        private void GetPortName()
        {
            string[] portNames = SerialPort.GetPortNames();
            foreach (string name in portNames)
            {
                PortName_ComboBox.Items.Add(name);
            }
            this.PortName_ComboBox.Text = global::HansLinkManage.Properties.Settings.Default.PortName;
        }

        private void SerialPortForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
            e.Cancel = false;
        }

        private void SerialPortForm_Load(object sender, EventArgs e)
        {
            OpenSerialPort();
            SetSendTextBox();
        }

        private void OpenSerialPort()
        {
            if (serialPort.IsOpen)
            {
                return;
            }
            try
            {
                LoadConfig();
                serialPort.DataReceived -= serialPort_DataReceived;
                serialPort.DataReceived += serialPort_DataReceived;
                serialPort.Encoding = Encoding.ASCII;
                serialPort.Open();
                if (serialPort.IsOpen)
                {
                    SetOpen();
                }
                else
                {
                    SetClose();
                }
            }
            catch
            {
                MainMessage.Text = "打开串口失败!";
            }
        }

        bool SerialPortIsReceiving = false;
        int ReceiveCount = 0;
        int receiveNum = 0;
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPortIsReceiving = true;
            bool right = false;

            try
            {
                if (HexDisplay_CheckBox.Checked)
                {
                    byte[] b = new byte[serialPort.BytesToRead];
                    ReceiveCount += b.Length;
                    serialPort.Read(b, 0, b.Length);
                    string str = "";
                    for (int i = 0; i < b.Length; i++)
                    {
                        if (b[i] == 44)
                            right = true;
                        if (right)
                        {
                            str += Convert.ToChar(b[i]);
                        }
                        // str += string.Format("{0:X2} ", b[i]);
                    }
                    AppendTextBox(str);
                    str = null;
                }
                else
                {
                    ReceiveCount += serialPort.BytesToRead;
                    receiveNum = serialPort.BytesToRead;
                    char[] data = new char[receiveNum];
                    serialPort.Read(data, 0, receiveNum);
                    string sReceiveData = new string(data);
                    //StringBuilder sb = new StringBuilder();
                    //foreach (char c in data)
                    //{
                    //    sb.Append(c);
                    //}
                    //string sReceiveData = sb.ToString();
                    AppendTextBox(sReceiveData);
                    //AppendTextBox(serialPort.ReadExisting());
                }
                SetReceiveCountLabel(ReceiveCount.ToString());
            }
            catch { }

            SerialPortIsReceiving = false;
        }

        delegate void SetTextCallback(string text);

        private void AppendTextBox(string text)
        {
            try
            {
                if (Receive_TextBox.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(AppendTextBox);
                    this.Invoke(d, text);
                }
                else
                {
                    Receive_TextBox.SuspendLayout();
                    if (text.Length == 1 && text[0] == (char)0x08)
                    {
                        if (Receive_TextBox.Text.Length > 0)
                        {
                            Receive_TextBox.SelectionStart = Receive_TextBox.Text.Length - 1;
                            Receive_TextBox.SelectionLength = 1;
                            Receive_TextBox.SelectedText = "";
                        }
                    }
                    else
                    {
                        Receive_TextBox.AppendText(text);
                    }
                    if (Receive_TextBox.Text.Length > 100000)
                    {
                        Receive_TextBox.Text = Receive_TextBox.Text.Substring(50000, Receive_TextBox.Text.Length - 50000);
                    }
                    Receive_TextBox.ResumeLayout(false);
                }
            }
            catch { }
        }

        private void SetReceiveCountLabel(string text)
        {
            try
            {
                if (ReceiveCount_Label.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetReceiveCountLabel);
                    this.Invoke(d, text);
                }
                else
                {
                    ReceiveCount_Label.Text = text;
                }
            }
            catch { }
        }

        private void SetSendTextBox()
        {
            if (HexSend_CheckBox.Checked)
            {
                SendHex_TextBox.Visible = true;
                SendHex_TextBox.Enabled = true;
                SendString_TextBox.Visible = false;
                SendString_TextBox.Enabled = false;
            }
            else
            {
                SendHex_TextBox.Visible = false;
                SendHex_TextBox.Enabled = false;
                SendString_TextBox.Visible = true;
                SendString_TextBox.Enabled = true;
            }
        }

        private void CloseSerialPort()
        {
            if (!serialPort.IsOpen)
            {
                return;
            }
            try
            {
                serialPort.DataReceived -= serialPort_DataReceived;
                while (SerialPortIsReceiving)
                {
                    Application.DoEvents();
                }
                serialPort.Close();
                if (serialPort.IsOpen)
                {
                    SetOpen();
                }
                else
                {
                    SetClose();
                }
            }
            catch
            {
                MainMessage.Text = "关闭串口失败!";
            }
        }

        private void LoadConfig()
        {
            PortName_ComboBox_TextChanged(null, null);
            BaudRate_ComboBox_TextChanged(null, null);
            DataBits_ComboBox_TextChanged(null, null);
            StopBits_ComboBox_TextChanged(null, null);
            Parity_ComboBox_TextChanged(null, null);
            DTR_CheckBox_CheckedChanged(null, null);
            RTS_CheckBox_CheckedChanged(null, null);
        }

        private void SetOpen()
        {
            OpenClosePort_Button.Text = "关闭串口";
            Light.ForeColor = Color.Lime;
            Com_Status.Text = serialPort.PortName + "已打开";
            BaudRate_Label.Text = serialPort.BaudRate.ToString();
        }

        private void SetClose()
        {
            OpenClosePort_Button.Text = "打开串口";
            Light.ForeColor = Color.DarkGray;
            Com_Status.Text = serialPort.PortName + "已关闭";
            BaudRate_Label.Text = serialPort.BaudRate.ToString();
        }

        private void PortName_ComboBox_TextChanged(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                CloseSerialPort();
                serialPort.PortName = PortName_ComboBox.Text;
                OpenSerialPort();
            }
            else
            {
                if (string.IsNullOrEmpty(PortName_ComboBox.Text))
                    return;
                serialPort.PortName = PortName_ComboBox.Text;
            }
        }

        private void BaudRate_ComboBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                serialPort.BaudRate = Convert.ToInt32(BaudRate_ComboBox.Text);
            }
            catch
            {
                MainMessage.Text = "波特率配置错误!";
            }
            BaudRate_Label.Text = serialPort.BaudRate.ToString();
        }

        private void DataBits_ComboBox_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(DataBits_ComboBox.Text))
                return ;
            serialPort.DataBits = Convert.ToInt32(DataBits_ComboBox.Text);
        }

        private void StopBits_ComboBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (StopBits_ComboBox.Text == "0")
                    serialPort.StopBits = StopBits.None;
                else if (StopBits_ComboBox.Text == "1")
                    serialPort.StopBits = StopBits.One;
                else if (StopBits_ComboBox.Text == "2")
                    serialPort.StopBits = StopBits.OnePointFive;
                else if (StopBits_ComboBox.Text == "3")
                    serialPort.StopBits = StopBits.Two;
            }
            catch
            {
                MainMessage.Text = "停止位配置错误!";
            }
        }

        private void Parity_ComboBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Parity_ComboBox.Text == "None")
                    serialPort.Parity = Parity.None;
                else if (Parity_ComboBox.Text == "Odd")
                    serialPort.Parity = Parity.Odd;
                else if (Parity_ComboBox.Text == "Even")
                    serialPort.Parity = Parity.Even;
                else if (Parity_ComboBox.Text == "Mark")
                    serialPort.Parity = Parity.Mark;
                else if (Parity_ComboBox.Text == "Space")
                    serialPort.Parity = Parity.Space;
            }
            catch
            {
                MainMessage.Text = "校验位配置错误!";
            }
        }

        private void DTR_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            serialPort.DtrEnable = DTR_CheckBox.Checked;
        }

        private void RTS_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            serialPort.RtsEnable = RTS_CheckBox.Checked;
        }

        private void OpenClosePort_Button_Click(object sender, EventArgs e)
        {
            if (OpenClosePort_Button.Text == "打开串口")
            {
                OpenSerialPort();
            }
            else
            {
                CloseSerialPort();
            }
        }

        private void Clearn_Button_Click(object sender, EventArgs e)
        {
            Receive_TextBox.Clear();
        }

        private void Send_Button_Click(object sender, EventArgs e)
        {
            SerialPortSend();
        }
        int SendCount = 0;
        private void SerialPortSend()

        {
            if (!serialPort.IsOpen)
            {
                MainMessage.Text = "串口未打开,无法发送数据!";
                return;
            }
            try
            {
                if (HexSend_CheckBox.Checked)
                {
                    byte[] byte_arr = new byte[SendHex_TextBox.Text.Length / 3];
                    for (int i = 0; i < SendHex_TextBox.Text.Length; i += 3)
                    {
                        try
                        {
                            byte_arr[i / 3] = Convert.ToByte(SendHex_TextBox.Text.Substring(i, 2), 16);
                        }
                        catch
                        {

                        }
                    }
                    serialPort.Write(byte_arr, 0, byte_arr.Length);
                    SendCount += byte_arr.Length;
                    SendCount_Label.Text = SendCount.ToString();
                }
                else
                {
                    string s = SendString_TextBox.Text;
                    if (s == "0")
                        s = "%01#WCSR01001**\r";
                    if (s == "7")
                        s = "%01#RCP7R0100R0101R0102R0103R0104R0105R0106**\r";
                    if (s == "OK")
                        s = "%01#WCP4R01011R01020R01030R01060**\r";
                    if (s == "NG")
                        s = "%01#WCP4R01010R01021R01030R01060**\r";
                    if (s == "LEAK")
                        s = "%01#WCP4R01011R01020R01030R01061**\r";
                    if (s.Contains("\\r"))
                    {
                        s = s.Replace(@"\r", "\r");
                    }
                    char[] char_arr = s.ToCharArray();
                    serialPort.Write(char_arr, 0, char_arr.Length);
                    SendCount += char_arr.Length;
                    SendCount_Label.Text = SendCount.ToString();
                }
                
            }
            catch
            {
                MainMessage.Text = "发送失败!";
            }
        }

        private void AutoSend_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoSend_CheckBox.Checked)
            {
                timer1.Interval = (int)Cycle_NumericUpDown.Value;
                timer1.Start();
            }
            else
            {
                timer1.Stop();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SerialPortSend();
            timer1.Interval = (int)Cycle_NumericUpDown.Value;
        }

        private void MainMessage_TextChanged(object sender, EventArgs e)
        {
            if (MainMessage.Text == "就绪")
            {
                return;
            }
            else
            {
                MainMessage.ForeColor = Color.Red;
                timer2.Start();
                timer2.Interval = 2000;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            MainMessage.Text = "就绪";
            MainMessage.ForeColor = Color.Black;
            timer2.Stop();
        }

        private void SerialPortForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseSerialPort();
            MainForm._mainForm.OpenSerialPortM();
            MainForm._mainForm.threadState = true;
        }
    }
}



