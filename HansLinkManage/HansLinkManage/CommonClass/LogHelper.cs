using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HansLinkManage.CommonClass
{
    public class LogHelper
    {
        static string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
        public static bool RecordLog = true;
        public static bool DebugLog = false;

        private static object lockobj = new object();
        private string logDirectory;
        private string loggerDate;
        private string loggerFile;
        private string loggerName;
        private string logThisDirectory;
        public LogHelper(string loggerName)
        {
            //if (!Directory.Exists(LogFolder))
            //{
            //    Directory.CreateDirectory(LogFolder);
            //}
            if (!string.IsNullOrEmpty(loggerName))
            {
                this.loggerName = loggerName;
            }
            else
            {
                this.loggerName = "DefaultLogger";
            }
            // this.logDirectory = new FileInfo(Assembly.GetExecutingAssembly().GetName().CodeBase.Replace("file:///", string.Empty)).DirectoryName + @"\Log";
            this.logDirectory = new FileInfo(AppDomain.CurrentDomain.BaseDirectory).DirectoryName + @"\Log";
            //this.logDirectory= Application.StartupPath+ @"\Log";
            if (!Directory.Exists(this.logDirectory))
            {
                Directory.CreateDirectory(this.logDirectory);
            }
        }

        public void Write(string line)
        {
            try
            {
                lock (lockobj)
                {
                    string str = DateTime.Now.ToString("yyyy-MM-dd");
                    if ((this.loggerFile == "") || !str.Equals(this.loggerDate))
                    {
                        this.loggerDate = str;
                        this.logThisDirectory = this.logDirectory + @"\" + this.loggerDate;
                        if (!Directory.Exists(this.logThisDirectory))
                        {
                            Directory.CreateDirectory(this.logThisDirectory);
                        }
                        this.loggerFile = this.logThisDirectory + @"\" + this.loggerName + ".log";
                    }
                    if (File.Exists(this.loggerFile))
                    {
                        //判断如果超过1M就进行文件分割
                        FileInfo file = new FileInfo(this.loggerFile);
                        if (file.Length > 1048576)
                        {
                            file.CopyTo(this.logThisDirectory + @"\" + this.loggerName + DateTime.Now.ToString("hhmmss") + ".log", true);
                            file.Delete();
                        }
                        using (StreamWriter writer = File.AppendText(this.loggerFile))
                            writer.WriteLine(string.Format("{0} {1} {2}", DateTime.Now.ToString("[yyyy-MM-dd HH: mm:ss]    "), Thread.CurrentThread.Name, line));
                    }

                    if (!File.Exists(this.loggerFile))
                        using (StreamWriter writer = File.CreateText(this.loggerFile))
                            writer.WriteLine(string.Format("{0}\r\n {1} {2}", DateTime.Now.ToString("[yyyy-MM-dd HH: mm:ss]    "), Thread.CurrentThread.Name, line));
                }
            }
            catch (Exception exception)
            {
                using (StreamWriter writer2 = File.Exists("log.txt") ? File.AppendText("log.txt") : File.CreateText("log.txt"))
                {
                    try
                    {
                        writer2.WriteLine(exception.ToString());
                        writer2.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void Write(string line, params object[] objects)
        {
            this.Write(string.Format(line, objects));
        }


        private  void WriteLine(string message)
        {
            string temp = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]    ") + message + "\r\n\r\n";
            string fileName = DateTime.Now.ToString("yyyyMMdd") + ".log";
            try
            {
                if (RecordLog)
                {
                    File.AppendAllText(Path.Combine(LogFolder, fileName), temp, Encoding.GetEncoding("GB2312"));
                }
                if (DebugLog)
                {
                    Console.WriteLine(temp);
                }
            }
            catch
            {
            }
        }

        public  void WriteLine(string className, string funName, string message)
        {
            WriteLine(string.Format("{0}：{1}\r\n{2}", className, funName, message));
        }

        public void EncWrite(string line)
        {
            try
            {
                lock (lockobj)
                {
                    string str = DateTime.Now.ToString("yyyy-MM-dd");
                    if ((this.loggerFile == "") || !str.Equals(this.loggerDate))
                    {
                        this.loggerDate = str;
                        this.logThisDirectory = this.logDirectory + @"\" + this.loggerDate;
                        if (!Directory.Exists(this.logThisDirectory))
                        {
                            Directory.CreateDirectory(this.logThisDirectory);
                        }
                        this.loggerFile = this.logThisDirectory + @"\" + this.loggerName + ".log";
                    }
                    if (File.Exists(this.loggerFile))
                    {
                        //判断如果超过1M就进行文件分割
                        FileInfo file = new FileInfo(this.loggerFile);
                        if (file.Length > 1048576)
                        {
                            file.CopyTo(this.logThisDirectory + @"\" + this.loggerName + DateTime.Now.ToString("hhmmss") + ".log", true);
                            file.Delete();
                        }
                        using (StreamWriter writer = File.AppendText(this.loggerFile))
                        {
                            string stre = DateTime.Now.ToString("[yyyy-MM-dd HH: mm:ss] ")  + line+ "\r\n";
                            string stree = ToEncrypt("9685", stre);
                            writer.WriteLine(string.Format("{0} {1}",  Thread.CurrentThread.Name, stree));
                            //writer.WriteLine(string.Format("{0} {1} {2}", DateTime.Now.ToString("[yyyy-MM-dd HH: mm:ss]    "), Thread.CurrentThread.Name, line));
                        }                           
                    }

                    if (!File.Exists(this.loggerFile))
                        using (StreamWriter writer = File.CreateText(this.loggerFile))
                        {
                            string stre = DateTime.Now.ToString("[yyyy-MM-dd HH: mm:ss] ")  + line+ "\r\n";
                            string stree = ToEncrypt("9685", stre);
                            writer.WriteLine(string.Format("{0} {1}", Thread.CurrentThread.Name, stree));
                            //writer.WriteLine(string.Format("{0} {1} {2}", DateTime.Now.ToString("[yyyy-MM-dd HH: mm:ss]    "), Thread.CurrentThread.Name, line));
                        }

                }
            }
            catch (Exception exception)
            {
                using (StreamWriter writer2 = File.Exists("log.txt") ? File.AppendText("log.txt") : File.CreateText("log.txt"))
                {
                    try
                    {
                        writer2.WriteLine(exception.ToString());
                        writer2.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        private string ToEncrypt(string encryptKey, string str)
        {
            try
            {
                byte[] P_byte_key = //将密钥字符串转换为字节序列
                    Encoding.Unicode.GetBytes(encryptKey);
                byte[] P_byte_data = //将字符串转换为字节序列
                    Encoding.Unicode.GetBytes(str);
                MemoryStream P_Stream_MS = //创建内存流对象
                    new MemoryStream();
                CryptoStream P_CryptStream_Stream = //创建加密流对象
                    new CryptoStream(P_Stream_MS, new DESCryptoServiceProvider().
                   CreateEncryptor(P_byte_key, P_byte_key), CryptoStreamMode.Write);
                P_CryptStream_Stream.Write(//向加密流中写入字节序列
                    P_byte_data, 0, P_byte_data.Length);
                P_CryptStream_Stream.FlushFinalBlock();//将数据压入基础流
                byte[] P_bt_temp =//从内存流中获取字节序列
                    P_Stream_MS.ToArray();
                P_CryptStream_Stream.Close();//关闭加密流
                P_Stream_MS.Close();//关闭内存流
                return //方法返回加密后的字符串
                    Convert.ToBase64String(P_bt_temp);
            }
            catch (CryptographicException ce)
            {
                throw new Exception(ce.Message);
            }
        }

        private string ToDecrypt(string encryptKey, string str)
        {
            try
            {
                byte[] P_byte_key = //将密钥字符串转换为字节序列
                    Encoding.Unicode.GetBytes(encryptKey);
                byte[] P_byte_data = //将加密后的字符串转换为字节序列
                    Convert.FromBase64String(str);
                MemoryStream P_Stream_MS =//创建内存流对象并写入数据
                    new MemoryStream(P_byte_data);
                CryptoStream P_CryptStream_Stream = //创建加密流对象
                    new CryptoStream(P_Stream_MS, new DESCryptoServiceProvider().
                    CreateDecryptor(P_byte_key, P_byte_key), CryptoStreamMode.Read);
                byte[] P_bt_temp = new byte[200];//创建字节序列对象
                MemoryStream P_MemoryStream_temp =//创建内存流对象
                    new MemoryStream();
                int i = 0;//创建记数器
                while ((i = P_CryptStream_Stream.Read(//使用while循环得到解密数据
                    P_bt_temp, 0, P_bt_temp.Length)) > 0)
                {
                    P_MemoryStream_temp.Write(//将解密后的数据放入内存流
                        P_bt_temp, 0, i);
                }
                return //方法返回解密后的字符串
                    Encoding.Unicode.GetString(P_MemoryStream_temp.ToArray());
            }
            catch (CryptographicException ce)
            {
                throw new Exception(ce.Message);
            }
        }
    }
}
