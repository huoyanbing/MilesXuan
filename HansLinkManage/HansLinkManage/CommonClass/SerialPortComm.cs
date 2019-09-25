using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HansLinkManage.CommonClass
{
    public class SerialPortComm
    {
        #region 变量声明区
        private const string PLCResponse = "%01$WC14\r";
        private SerialPort _serialPort;
        private string _portName;   //串口名称
        private int _baudRate;      //串口波特率
        private int _dataBits;      //数据位
        private Parity _parity;     //校验位
        private StopBits _stopBits; //停止位
        private bool _isOpen;       //是否打开

        public static object obj = new object();

        public string SendStr = "";   //待发送的字符串
        public Byte[] Buffer = new Byte[1024];    //接收到的字节
        //  public Char[] StrBuf = new Char[1024];    //接收到的字符
        public Char[] StrBuf;
        public string PortName
        {
            get
            {
                return _portName;
            }
            set
            {
                _portName = value;
            }
        }
        public int BaudRate
        {
            get
            {
                return _baudRate;
            }
            set
            {
                _baudRate = value;
            }
        }
        public int DataBits
        {
            get
            {
                return _dataBits;
            }
            set
            {
                _dataBits = value;
            }
        }
        public Parity Parity
        {
            get
            {
                return _parity;
            }
            set
            {
                _parity = value;
            }
        }
        public StopBits StopBits
        {
            get
            {
                return _stopBits;
            }
            set
            {
                _stopBits = value;
            }
        }
        public bool IsOpen
        {
            get
            {
                return _serialPort.IsOpen;
            }
        }
        #endregion

        #region 构造函数
        public SerialPortComm()  //构造函数
        {
            _serialPort = new SerialPort();
            _serialPort.ReadTimeout = 100;
        }
        #endregion

        #region 设置串口
    public void SetSerialPort()
        {
            _serialPort.PortName = _portName;
            _serialPort.BaudRate = _baudRate;
            _serialPort.DataBits = _dataBits;
            _serialPort.Parity = _parity;
            _serialPort.StopBits = _stopBits;
        }
        #endregion

        #region 打开串口0成功1失败
        public int OpenSerialPort() 
        {
            try
            {
                _serialPort.Open();
            }
            catch
            {
                return 1;
            }
            return 0;
        }
        #endregion

        #region 关闭串口0成功1失败
        public int CloseSerialPort()    
        {
            try
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();
            }
            catch
            {
                return 1;
            }

            return 0;
        }
        #endregion

        #region 将用户输入的字符串数，是否以十六进制发送
        public int SendData(bool _hex)  
        {
            try
            {
                if (_hex)//以16进制发送
                {
                    string[] Str = SendStr.Trim().Split(' ');
                    Byte[] buf = new Byte[1024];
                    int i = 0;
                    foreach (string s in Str)
                    {
                        buf[i++] = Convert.ToByte(s, 16);
                    }
                    _serialPort.Write(buf, 0, i);
                }
                else//以ASCII码格式发送
                {
                    char[] chs = SendStr.ToCharArray();
                    _serialPort.Write(chs, 0, chs.Length);
                }
            }
            catch
            {
                return 1;
            }
            return 0;
        }
        #endregion

        #region 接收数据
        public int RecvData(bool _hex)
        {
            int _bytes = 0;
            try
            {
                if (_hex)
                {
                    _bytes = _serialPort.Read(Buffer, 0, _serialPort.BytesToRead);
                }
                else
                {
                    int receiveNum = _serialPort.BytesToRead;
                    StrBuf = new char[receiveNum];
                    _bytes = _serialPort.Read(StrBuf, 0, receiveNum);
                   // _bytes = _serialPort.Read(StrBuf, 0, _serialPort.BytesToRead);
                }
            }
            catch
            {
                return 0;
            }
            return _bytes;
        }
        #endregion

        #region 获得当前打开串口参数
        public int GetSetting()
        {
            _portName = _serialPort.PortName;
            _baudRate = _serialPort.BaudRate;
            _dataBits = _serialPort.DataBits;
            _parity = _serialPort.Parity;
            _stopBits = _serialPort.StopBits;
            _isOpen = _serialPort.IsOpen;
            return 0;
        }
        #endregion

        #region 清除接收缓冲区的数据
        public void DiscardInBuffer()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.DiscardInBuffer();
            }
        }
        #endregion

        #region 清除传输缓冲区的数据
        public void DiscardOutBuffer()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.DiscardOutBuffer();
            }
        }
        #endregion
        public int Data_Write(string data, ref string ret)
        {
            SendStr = data;
            int bytes = 0, p = 0;
            if (1 == SendData(false))
            {
                return 1;//发送失败
            }
            Thread.Sleep(100);
            while (bytes == 0 && p < 3)//重试100次
            {
                bytes = RecvData(false);
                p++;
                Thread.Sleep(20);
            }
            if (bytes == 0)
            {
                return 1;
            }
            ret = "";
            ret = new string(StrBuf);           
            ret = ret.TrimEnd('\0');
            return 0;
        }

        public int Data_Read(string addr, ref string data)
        {
            SendStr = addr;
            int bytes = 0, p = 0;
            if (1 == SendData(false))
            {
                return 1;//发送失败
            }
            Thread.Sleep(100);
            while (bytes == 0 && p < 3)
            {
                bytes = RecvData(false);
                p++;
                Thread.Sleep(20);
            }
            if (bytes == 0)
            {
                return 1;
            }
            data = "";
            data = new string(StrBuf);
            data = data.TrimEnd('\0');
            return 0;
        }

        public int WriteMultiSinglePoint(string sendData)
        {
            try
            {
                
                string recvData = "";
                int p = 0;//重试次数
                while (p < 3)
                {
                    Monitor.Enter(obj);
                    int nStatus = Data_Write(sendData, ref recvData);
                    Monitor.Exit(obj);
                    if (0 == nStatus)
                    {
                        break;
                    }
                    p++;
                }
                if (3 == p)
                {
                    return 1;//数据通讯失败
                }
                if (recvData.CompareTo(PLCResponse) !=0 )
                   return 1;
            }
            catch
            {
                return 1;
            }
            return 0;
        }

        public int ReadMultiSinglePoint(string sendData,int nRCP,ref string strRecv)
        {
            try
            {
                string recvData = "";
                int p = 0;//重试次数
                while (p < 3)
                {
                    Monitor.Enter(obj);
                    int nStatus = Data_Read(sendData, ref recvData);
                    Monitor.Exit(obj);
                    if (0 == nStatus)
                    {
                        break;
                    }
                    p++;
                }
                if (3 == p)
                {
                    return 1;//数据通讯失败
                }
                if(recvData.Length==(nRCP+9)&& recvData.StartsWith("%")&& recvData.EndsWith("\r"))
                {
                    strRecv = recvData.Substring(6, nRCP);
                }
                else
                {
                    return 1;
                }

            }
            catch
            {
                return 1;
            }
            return 0;
        }


    }

}
