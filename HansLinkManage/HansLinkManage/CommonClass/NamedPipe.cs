using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HansLinkManage.CommonClass
{
    public class AsyncState
    {
        public byte[] Buffer { get; set; }
        public NamedPipeServerStream Stream { get; set; }
        public ManualResetEvent EvtHandle { get; set; }
        public MemoryStream MemoryStream { get; set; }
    }

    public delegate void ReadFPTHandler(string strfpt);
    public delegate void ShowPipeLEDHandler();
    public class NamedPipe
    {
        public static NamedPipeServerStream pipeStream;
        public const string STR_END = "\0";
        private byte[] BufPipe;
        private int BufferSize = 1024;
       // public SerialPortComm NPserialPortComm;
        public delegate void MainPictureHandler();
        public event ReadFPTHandler ReadFPTEvent;
        public event ShowPipeLEDHandler ShowPipeLEDOnvent, ShowPipeLEDOffvent;


        #region 构造函数
        public NamedPipe()
        {
           // NPserialPortComm = _serialPortComm;
        }
        #endregion
        public static NamedPipeServerStream OnCreatePipe()
        {
            NamedPipeServerStream PipeStream = new NamedPipeServerStream("MyPipe", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
                                                   PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            PipeStream.WaitForConnection();
            return PipeStream;
        }

        public void ConnectPipeThread(object state)
        {
            while (true)
            {
                pipeStream = OnCreatePipe();
                //MainForm._mainForm.Invoke(new MainPictureHandler(MainForm._mainForm.MainPictureON));
                ShowPipeLedOnRun();
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReadPipeThread), pipeStream);
            }
        }

        public int OnPipeWrite(string message)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] bytes= encoding.GetBytes(message);
            try
            {
                if (pipeStream == null)
                    return 1;
                if (pipeStream.IsConnected)
                {
                    pipeStream.Write(bytes, 0, bytes.Length);
                    pipeStream.WaitForPipeDrain();
                }
                    
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
                pipeStream.Dispose();
                return 1;
            }
            return 0;
        }
        public void ReadPipeThread(object state)
        {
            NamedPipeServerStream PipeStream = state as NamedPipeServerStream;
            while (true)
            {
                BufPipe = new byte[BufferSize];
                AsyncState asyncState = new AsyncState()
                {
                    Buffer = BufPipe,
                    EvtHandle = new ManualResetEvent(false),
                    Stream = pipeStream
                };
                pipeStream.BeginRead(BufPipe, 0, BufPipe.Length, new AsyncCallback(ReadCallback), asyncState);
                asyncState.EvtHandle.WaitOne();
            }

        }

        private void ReadCallback(IAsyncResult arg)
        {
            AsyncState state = arg.AsyncState as AsyncState;
            int length = state.Stream.EndRead(arg);
            string readContent="";      
            if (length == 0)
            {
                // MainForm._mainForm.Invoke(new MainPictureHandler(MainForm._mainForm.MainPictureOFF));
                ShowPipeLedOffRun();
                pipeStream.Dispose();
                return;
            }
            if (length > 0)
            {

                byte[] buffer;
                if (length == BufferSize) buffer = state.Buffer;
                else
                {
                    buffer = new byte[length];
                    Array.Copy(state.Buffer, 0, buffer, 0, length);
                    readContent = Encoding.UTF8.GetString(buffer);
                   // MessageBox.Show(readContent);
                }
                // if (state.MemoryStream == null) state.MemoryStream = new MemoryStream();
                //  state.MemoryStream.Write(buffer, 0, buffer.Length);
                //  state.MemoryStream.Flush();

            }
            if (length < BufferSize)
            {
                /*
                state.MemoryStream.Position = 0;
                using (StreamReader reader = new StreamReader(state.MemoryStream))
                {
                    this.InputStr.Append(reader.ReadToEnd());

                }
                state.MemoryStream.Close();
                state.MemoryStream.Dispose();
                state.MemoryStream = null;
                */

                ReadFPTRun(readContent);

                /*
                if(MainForm.blpipe)
                {
                    if (readContent.CompareTo("TESTOK") == 0)
                    {  
                       if (1 == NPserialPortComm.WriteMultiSinglePoint("%%03#WCSR1011**\r"))
                         MessageBox.Show("Send TESTOK Fail");
                    }
                    if (readContent.CompareTo("TESTNG") == 0)
                    {
                       if (1 == NPserialPortComm.WriteMultiSinglePoint("%%03#WCSR1021**\r"))
                         MessageBox.Show("Send TESTNG Fail");
                    }
                }  */
                state.EvtHandle.Set();

            }
            else
            {
                Array.Clear(state.Buffer, 0, BufferSize);
                //再次执行异步读取操作
                state.Stream.BeginRead(state.Buffer, 0, BufferSize, new AsyncCallback(ReadCallback), state);
            }

        }

        private void ReadFPTRun(string str)
        {
            if(ReadFPTEvent!=null)
            {
                ReadFPTEvent(str);
            }
        }

        private void ShowPipeLedOnRun()
        {
            if(ShowPipeLEDOnvent != null)
            {
                ShowPipeLEDOnvent();
            }
        }

        private void ShowPipeLedOffRun()
        {
            if (ShowPipeLEDOffvent != null)
            {
                ShowPipeLEDOffvent();
            }
        }
    }
}
