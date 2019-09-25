using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace FPT.CommonClass
{
    
    public class FileOperate
    {
        #region 变量声明区
        private string m_IniFileName;  //该变量保存INI文件所在的具体物理位置
        private string m_AllPath = "";//记录选择路径
        public static string inipath = "";
        public delegate void GetListViewFileHandler(string path, ImageList imglist, ListView lv);
        public GetListViewFileHandler myListViewDelegate, myListViewWTDelegate, myListViewOneDelegate;
        private int m_FileNum;
        private string[] m_fileName;
        public bool blListViewRun = false;
        public string IniFileName
        {
            get { return m_IniFileName; }
            set { m_IniFileName = value; }
        }

        public string AllPath
        {
            get { return m_AllPath; }
            set { m_AllPath = value; }
        }

        public int FileNum
        {
            get { return m_FileNum; }
            set { m_FileNum = value; }
        }

        public string strOne = "";
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            int nSize,
            string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileInt(
            string lpAppName,
            string lpKeyName,
            int nDefault,
            string lpFileName
            );

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(
            string mpAppName,
            string mpKeyName,
            string mpDefault,
            string mpFileName);

        [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttribute, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint Flags);

        [DllImport("User32.dll", EntryPoint = "DestroyIcon")]
        public static extern int DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;//文件的图标句柄
            public IntPtr iIcon;//图标的系统索引号
            public uint dwAttributes;//文件的属性值
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;//文件的显示名
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;//文件的类型名
        }
        #endregion
        #region 构造函数
        public FileOperate()
        {
            myListViewDelegate = new GetListViewFileHandler(GetListViewItemFile);
            myListViewWTDelegate = new GetListViewFileHandler(GetListViewItemFileWT);
            myListViewOneDelegate = new GetListViewFileHandler(GetListViewOneFile);
        }
        #endregion

        #region 读取string字符串
        public string ReadIniData(string Section, string Key, string NoText)
        {
            if (File.Exists(this.m_IniFileName))
            {
                StringBuilder temp = new StringBuilder(1024);
                GetPrivateProfileString(Section, Key, NoText, temp, 1024, this.m_IniFileName);
                return temp.ToString();
            }
            else
            {
                return String.Empty;
            }
        }
        #endregion

        #region 读Int数值 

        public int ReadIniInt(string Section, string Key)
        {
            if (File.Exists(this.m_IniFileName))
                return GetPrivateProfileInt(Section, Key, -1, this.m_IniFileName);
            else
                return -2;
        }
        #endregion

        #region 写入String字符串，如果不存在 节-键，则会自动创建
        public  bool WriteIniData(string Section, string Key, string Value)
        {
            if (File.Exists(this.m_IniFileName))
            {
                long OpStation = WritePrivateProfileString(Section, Key, Value, this.m_IniFileName);
                if (OpStation == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 写入Int数值，如果不存在 节-键，则会自动创建

        public bool WriteIniInt(string Section, string Key, int Ival)
        {

            if (File.Exists(this.m_IniFileName))
            {
                long OpStation = WritePrivateProfileString(Section, Key, Ival.ToString(), this.m_IniFileName);
                if (OpStation == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 删除指定节
        public void DeleteSection(string Section)
        {
            WritePrivateProfileString(Section, null, null, this.m_IniFileName);
        }
        #endregion

        #region 删除全部节
        public void DeleteAllSection()
        {
            WritePrivateProfileString(null, null, null, this.m_IniFileName);
        }
        #endregion

        #region 删除指定键
        public void Deletekey(string Section, string Key)
        {
            WritePrivateProfileString(Section, Key, null, this.m_IniFileName);
        }
        #endregion

        #region  检测指定目录是否存在
        public  bool IsExistDirectory(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }
        #endregion

        #region 获取指定目录中的文件列表
        public  string[] GetFileNames(string directoryPath, bool searchPattern)
        {
            //如果目录不存在，则抛出异常
            if (!IsExistDirectory(directoryPath))
            {
                throw new FileNotFoundException();
            }
            //获取文件列表
            return Directory.GetFiles(directoryPath);
        }
        #endregion

        #region 获取指定目录及子目录中所有文件列表
        public  string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
        {
            //如果目录不存在，则抛出异常
            if (!IsExistDirectory(directoryPath))
            {
                throw new FileNotFoundException();
            }

            try
            {
                if (isSearchChild)
                {
                    return Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
                }
                else
                {
                    return Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly); 
                }
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 获取指定目录356专用
        public string[] GetFileNames356(string directoryPath)
        {
            //如果目录不存在，则抛出异常
            if (!IsExistDirectory(directoryPath))
            {
                throw new FileNotFoundException();
            }

            try
            {             
                    return (string[])Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".emm") || s.EndsWith(".356")).ToArray(); 
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 创建文件或文件夹
        public  void CreateFileDir(string strName, int intflag)
        {
            string strPath = this.m_AllPath + "\\"+strName;
            if (intflag == 0)
            {
                if (File.Exists(strPath)==false)
                    File.Create(strPath);//新建文件
            }
            else if (intflag == 1)
            {
               if(Directory.Exists(strPath)==false)
                    Directory.CreateDirectory(strPath);//新建文件夹
            }
        }
        #endregion

        #region 获取指定目录中的文件夹列表
        public  string[] GetFolderNames(string directoryPath)
        {
            //如果目录不存在，则抛出异常
            if (!IsExistDirectory(directoryPath))
            {
                throw new FileNotFoundException();
            }
            //获取文件列表
            return Directory.GetDirectories(directoryPath);
        }
        #endregion

         #region 获取文件夹名字
        public string GetDirectoryNames(string directoryPath)
        {
            DirectoryInfo dir = new DirectoryInfo(directoryPath);
            return dir.Name;
        }
         #endregion

        #region 获取指定路径下所有文件及其图标
        /// <summary>
        /// 获取指定路径下所有文件及其图标
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="imglist">ImageList对象</param>
        /// <param name="lv">ListView对象</param>
        public void GetListViewItem(string path, ImageList imglist, ListView lv)
        {
            lv.Items.Clear();
            SHFILEINFO shfi = new SHFILEINFO();
            try
            {
                string[] dirs = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);
                for (int i = 0; i < dirs.Length; i++)
                {
                    string[] info = new string[4];
                    DirectoryInfo dir = new DirectoryInfo(dirs[i]);
                    if (dir.Name == "RECYCLER" || dir.Name == "RECYCLED" || dir.Name == "Recycled" || dir.Name == "System Volume Information")
                    { }
                    else
                    {
                        //获得图标
                        SHGetFileInfo(dirs[i],
                                            (uint)0x80,
                                            ref shfi,
                                            (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                                            (uint)(0x100 | 0x400));//取得Icon和TypeName
                        //添加图标
                        imglist.Images.Add(dir.Name, (Icon)Icon.FromHandle(shfi.hIcon).Clone());
                        info[0] = dir.Name;
                        info[1] = "";
                        info[3] = "文件夹";
                        info[2] = dir.LastWriteTime.ToString();
                        ListViewItem item = new ListViewItem(info, dir.Name);
                        lv.Items.Add(item);
                        //销毁图标
                        DestroyIcon(shfi.hIcon);
                    }
                }
                for (int i = 0; i < files.Length; i++)
                {
                    string[] info = new string[5];
                    FileInfo fi = new FileInfo(files[i]);
                    string Filetype = fi.Name.Substring(fi.Name.LastIndexOf(".") + 1, fi.Name.Length - fi.Name.LastIndexOf(".") - 1);
                    string newtype = Filetype.ToLower();
                    if (newtype == "sys" || newtype == "ini" || newtype == "bin" || newtype == "log" || newtype == "com" || newtype == "bat" || newtype == "db")
                    { }
                    else
                    {
                        //获得图标
                        SHGetFileInfo(files[i],
                                            (uint)0x80,
                                            ref shfi,
                                            (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                                            (uint)(0x100 | 0x400)); //取得Icon和TypeName
                        //添加图标
                        imglist.Images.Add(fi.Name, (Icon)Icon.FromHandle(shfi.hIcon).Clone());
                        info[0] = fi.Name;
                        /*
                        double dbLength = fi.Length / 1024;
                        if (dbLength < 1024)
                            info[1] = dbLength.ToString("0.00") + " KB";
                        else
                            info[1] = Convert.ToDouble(dbLength / 1024).ToString("0.00") + " MB";
                         */
                        double dbLength = Math.Ceiling((double)fi.Length / 1024);
                        if (dbLength < 1024)
                            info[1] = dbLength.ToString("F0") + " KB";
                        else
                            info[1] = Convert.ToDouble(dbLength / 1024).ToString("F0") + " MB";
                        info[3] = fi.Extension.ToString();
                        info[2] = fi.LastWriteTime.ToString();
                        info[4] = fi.IsReadOnly.ToString();
                        ListViewItem item = new ListViewItem(info, fi.Name);
                        lv.Items.Add(item);
                        //销毁图标
                        DestroyIcon(shfi.hIcon);
                    }
                }
            }
            catch
            {
            }
        }
        #endregion

        #region 将指定路径的下的文件及文件夹显示在ListView控件中
        /// <summary>
        /// 将指定路径的下的文件及文件夹显示在ListView控件中
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="imglist">ImageList控件对象</param>
        /// <param name="lv">ListView控件对象</param>
        /// <param name="ppath">标识要执行的操作</param>
        public void GetPath(string path, ImageList imglist, ListView lv, int intflag)
        {
            string pp = "";
            string uu = "";
            try
            {
                if (intflag == 0)
                {
                    if (AllPath != path)
                    {
                        lv.Items.Clear();
                        AllPath = path;
                        GetListViewItem(AllPath, imglist, lv);
                    }
                }
                else
                {
                    uu = AllPath + path;
                    if (Directory.Exists(uu))
                    {
                        AllPath = AllPath + path + "\\";
                        pp = AllPath.Substring(0, AllPath.Length - 1);
                        lv.Items.Clear();
                        GetListViewItem(pp, imglist, lv);
                    }
                    else
                    {
                        if (path.IndexOf("\\") == -1)//判断如果不是完整路径，先转换为完整路径，再打开
                        {
                            uu = AllPath + path;
                            System.Diagnostics.Process.Start(uu);
                        }
                        else//判断如果是完整路径，则直接打开
                            System.Diagnostics.Process.Start(path);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 获取指定路径下所有文件夹图标
        /// <summary>
        /// 获取指定路径下所有文件夹图标
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="imglist">ImageList对象</param>
        /// <param name="lv">ListView对象</param>
        public void GetListViewItemDir(string path, ImageList imglist, ListView lv)
        {
            lv.Items.Clear();
            SHFILEINFO shfi = new SHFILEINFO();
            try
            {
                string[] dirs = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);
                for (int i = 0; i < dirs.Length; i++)
                {
                    string[] info = new string[4];
                    DirectoryInfo dir = new DirectoryInfo(dirs[i]);
                    if (dir.Name == "RECYCLER" || dir.Name == "RECYCLED" || dir.Name == "Recycled" || dir.Name == "System Volume Information")
                    { }
                    else
                    {
                        //获得图标
                        SHGetFileInfo(dirs[i],
                                            (uint)0x80,
                                            ref shfi,
                                            (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                                            (uint)(0x100 | 0x400));//取得Icon和TypeName
                        //添加图标
                        imglist.Images.Add(dir.Name, (Icon)Icon.FromHandle(shfi.hIcon).Clone());
                        info[0] = dir.Name;
                        info[1] = dir.CreationTime.ToString();
                        info[2] = "文件夹";
                       // info[3] = dir.LastWriteTime.ToString();
                        ListViewItem item = new ListViewItem(info, dir.Name);
                        lv.Items.Add(item);
                        //销毁图标
                        DestroyIcon(shfi.hIcon);
                    }
                }
            }
            catch
            {
            }
        }
        #endregion

        #region 获取指定路径下特定文件图标
        public void GetListViewItemFile(string path, ImageList imglist, ListView lv)
        {
            blListViewRun = true;
            SHFILEINFO shfi = new SHFILEINFO();
            try
            {
                string[] files = Directory.GetFiles(path);
                if(m_FileNum== files.Length)
                {
                    if (Enumerable.SequenceEqual(m_fileName, files))
                    {
                        blListViewRun = false;
                        return;
                    }
                        
                    else
                    {
                        lv.Items.Clear();
                        m_fileName = new string[files.Length];
                        m_fileName = files;
                        m_FileNum = files.Length;
                    }
                }
                else
                {
                    lv.Items.Clear();
                    m_fileName = new string[files.Length];
                    m_fileName = files;
                    m_FileNum = files.Length;
                }
               // string filename = System.IO.Path.GetFileName(files[files.Length - 1]);
               
                for (int i = 0; i < files.Length; i++)
                {
                    string[] info = new string[5];
                    FileInfo fi = new FileInfo(files[i]);
                    string Filetype = fi.Name.Substring(fi.Name.LastIndexOf(".") + 1, fi.Name.Length - fi.Name.LastIndexOf(".") - 1);
                    string newtype = Filetype.ToLower();
                    if (/*newtype == "emm" || newtype == "356" || newtype == "ipc"*/newtype == "erp")
                    {
                        //获得图标
                        SHGetFileInfo(files[i],
                                            (uint)0x80,
                                            ref shfi,
                                            (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                                            (uint)(0x100 | 0x400)); //取得Icon和TypeName
                        //添加图标
                        imglist.Images.Add(fi.Name, (Icon)Icon.FromHandle(shfi.hIcon).Clone());
                        info[0] = fi.Name;
                        double  dbLength = Math.Ceiling((double)fi.Length / 1024);
                        if (dbLength < 1024)
                            info[2] = dbLength.ToString("F0") + " KB";
                        else
                            info[2] = Convert.ToDouble(dbLength / 1024).ToString("F0") + " MB";
                        info[3] = fi.Extension.ToString();
                        info[1] = fi.LastWriteTime.ToString();
                        info[4] = fi.IsReadOnly.ToString();                           
                        ListViewItem item = new ListViewItem(info, fi.Name);
                        lv.Items.Add(item);
                        //销毁图标
                        DestroyIcon(shfi.hIcon);
                    }
                }
            }
            catch
            {
            }
            blListViewRun = false;
        }
        #endregion

        #region 获取指定路径下特定文件图标
        public void GetListViewItemFileWT(string path, ImageList imglist, ListView lv)
        {
            lv.Items.Clear();
            SHFILEINFO shfi = new SHFILEINFO();
            try
            {
                string[] files = Directory.GetFiles(path);              
                for (int i = 0; i < files.Length; i++)
                {
                    string[] info = new string[5];
                    FileInfo fi = new FileInfo(files[i]);
                    string Filetype = fi.Name.Substring(fi.Name.LastIndexOf(".") + 1, fi.Name.Length - fi.Name.LastIndexOf(".") - 1);
                    string newtype = Filetype.ToLower();
                    if (/*newtype == "emm" || newtype == "356" || newtype == "ipc"*/newtype == "erp")
                    {
                        //获得图标
                        SHGetFileInfo(files[i],
                                            (uint)0x80,
                                            ref shfi,
                                            (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                                            (uint)(0x100 | 0x400)); //取得Icon和TypeName
                        //添加图标
                        imglist.Images.Add(fi.Name, (Icon)Icon.FromHandle(shfi.hIcon).Clone());
                        info[0] = fi.Name;
                        double dbLength = Math.Ceiling((double)fi.Length / 1024);
                        if (dbLength < 1024)
                            info[1] = dbLength.ToString("F0") + " KB";
                        else
                            info[1] = Convert.ToDouble(dbLength / 1024).ToString("F0") + " MB";
                        info[3] = fi.Extension.ToString();
                        info[2] = fi.LastWriteTime.ToString();
                        info[4] = fi.IsReadOnly.ToString();
                        ListViewItem item = new ListViewItem(info, fi.Name);
                        lv.Items.Add(item);
                        //销毁图标
                        DestroyIcon(shfi.hIcon);
                    }
                }
            }
            catch
            {
            }
        }
        #endregion

        #region 获取指定文件的特定文件图标
        public void GetListViewOneFile(string filepath, ImageList imglist, ListView lv)
        {
            //lv.Items.Clear();
            SHFILEINFO shfi = new SHFILEINFO();
            try
            {
                
                    string[] info = new string[5];
                    FileInfo fi = new FileInfo(filepath);
                    string Filetype = fi.Name.Substring(fi.Name.LastIndexOf(".") + 1, fi.Name.Length - fi.Name.LastIndexOf(".") - 1);
                    string newtype = Filetype.ToLower();
                    if (/*newtype == "emm" || newtype == "356" || newtype == "ipc"*/newtype == "erp")
                    {
                        //获得图标
                        SHGetFileInfo(filepath,
                                            (uint)0x80,
                                            ref shfi,
                                            (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                                            (uint)(0x100 | 0x400)); //取得Icon和TypeName
                        //添加图标
                        imglist.Images.Add(fi.Name, (Icon)Icon.FromHandle(shfi.hIcon).Clone());
                        info[0] = fi.Name;
                        double dbLength = Math.Ceiling((double)fi.Length / 1024);
                        if (dbLength < 1024)
                            info[1] = dbLength.ToString("F0") + " KB";
                        else
                            info[1] = Convert.ToDouble(dbLength / 1024).ToString("F0") + " MB";
                       // info[3] = fi.Extension.ToString();
                       // info[2] = fi.LastWriteTime.ToString();
                       // info[4] = fi.IsReadOnly.ToString();
                        ListViewItem item = new ListViewItem(info, fi.Name);
                        lv.Items.Add(item);
                        //销毁图标
                        DestroyIcon(shfi.hIcon);
                    }
                
            }
            catch
            {
            }
        }
        #endregion

        #region 从文件的绝对路径中获取文件名( 包含扩展名 )
        /// <summary>
        /// 9.1从文件的绝对路径中获取文件名( 包含扩展名 )
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>        
        public string GetFileName(string filePath)
        {
            //获取文件的名称
            FileInfo fi = new FileInfo(filePath);
            return fi.Name;
        }
        #endregion

        #region  检测指定文件是否存在,如果存在则返回true

        /// <summary>
        /// 3.2检测指定文件是否存在,如果存在则返回true
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>        
        public bool IsExistFile(string filePath)
        {
            return File.Exists(filePath);
        }
        #endregion

        #region 删除文件
        /// <summary>
        /// 6.2删除文件
        /// </summary>
        /// <param name="file">要删除的文件路径和名称</param>
        public void DeleteFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);         
        }
        #endregion

        #region 将文件移动到指定目录
        /// <summary>
        /// 8.4将文件移动到指定目录
        /// </summary>
        /// <param name="sourceFilePath">需要移动的源文件的绝对路径</param>
        /// <param name="descDirectoryPath">移动到的目录的绝对路径</param>
        public void Move(string sourceFilePath, string descDirectoryPath)
        {
            //获取源文件的名称
            string sourceFileName = GetFileName(sourceFilePath);

            if (Directory.Exists(descDirectoryPath))
            {
                //如果目标中存在同名文件,则删除
                if (IsExistFile(descDirectoryPath + "\\" + sourceFileName))
                {
                    DeleteFile(descDirectoryPath + "\\" + sourceFileName);
                }
                //将文件移动到指定目录
                File.Move(sourceFilePath, descDirectoryPath + "\\" + sourceFileName);
            }
        }
        #endregion

        # region 将数据转换为整型
        /// 将数据转换为整型   转换失败返回默认值
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="defValue">默认值</param>
        /// <returns></returns>
        public int ToInt32(string data, int defValue)
        {
            //如果为空则返回默认值
            if (string.IsNullOrEmpty(data))
            {
                return defValue;
            }

            int temp = 0;
            if (Int32.TryParse(data, out temp))
            {
                return temp;
            }
            else
            {
                return defValue;
            }
        }
        #endregion

        #region 获取文件个数
        public int GetFileCount(string dir, string ext)
        {
            int count = 0;
            DirectoryInfo d = new DirectoryInfo(dir);
            foreach (FileInfo fi in d.GetFiles())
            {
                if (fi.Extension.ToLower() == ext.ToLower())
                    count++;
            }
            return count;
        }
        #endregion
        public void ExecuteBatdFile()
        {
            Process proc = null;
            try
            {
                string targetDir = string.Format(@"C:\Program Files\Microsoft Visual Studio 14.0\Common7\Tools\");//this is where testChange.bat lies
                proc = new Process();
                proc.StartInfo.WorkingDirectory = targetDir;
                proc.StartInfo.FileName = "SelfCommBuildD.bat";
               // proc.StartInfo.Arguments = string.Format("10");//this is argument
                proc.StartInfo.UseShellExecute = true;        // 使用操作系统shell启动进程
                //proc.StartInfo.CreateNoWindow = true;
                //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//这里设置DOS窗口不显示，经实践可行
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }

        }

        public void ExecuteBateFile()
        {
            Process proc = null;
            try
            {
                string targetDir = string.Format(@"C:\Program Files\Microsoft Visual Studio 14.0\Common7\Tools\");//this is where testChange.bat lies
                proc = new Process();
                proc.StartInfo.WorkingDirectory = targetDir;
                proc.StartInfo.FileName = "SelfCommBuildE.bat";
                // proc.StartInfo.Arguments = string.Format("10");//this is argument
                proc.StartInfo.UseShellExecute = true;        // 使用操作系统shell启动进程
                //proc.StartInfo.CreateNoWindow = true;
                //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//这里设置DOS窗口不显示，经实践可行
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }

        }

        public void ExecuteBatfFile()
        {
            Process proc = null;
            try
            {
                string targetDir = string.Format(@"C:\Program Files\Microsoft Visual Studio 14.0\Common7\Tools\");//this is where testChange.bat lies
                proc = new Process();
                proc.StartInfo.WorkingDirectory = targetDir;
                proc.StartInfo.FileName = "SelfCommBuildF.bat";
                // proc.StartInfo.Arguments = string.Format("10");//this is argument
                proc.StartInfo.UseShellExecute = true;        // 使用操作系统shell启动进程
                //proc.StartInfo.CreateNoWindow = true;
                //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//这里设置DOS窗口不显示，经实践可行
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }

        }

        public void ExecuteBatgFile()
        {
            Process proc = null;
            try
            {
                string targetDir = string.Format(@"C:\Program Files\Microsoft Visual Studio 14.0\Common7\Tools\");//this is where testChange.bat lies
                proc = new Process();
                proc.StartInfo.WorkingDirectory = targetDir;
                proc.StartInfo.FileName = "SelfCommBuildG.bat";
                // proc.StartInfo.Arguments = string.Format("10");//this is argument
                proc.StartInfo.UseShellExecute = true;        // 使用操作系统shell启动进程
                //proc.StartInfo.CreateNoWindow = true;
                //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//这里设置DOS窗口不显示，经实践可行
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }

        }
        public void ExecuteBatlFile()
        {
            Process proc = null;
            try
            {
                string targetDir = string.Format(@"C:\Program Files\Microsoft Visual Studio 14.0\Common7\Tools\");//this is where testChange.bat lies
                proc = new Process();
                proc.StartInfo.WorkingDirectory = targetDir;
                proc.StartInfo.FileName = "SelfCommBuildL.bat";
                // proc.StartInfo.Arguments = string.Format("10");//this is argument
                proc.StartInfo.UseShellExecute = true;        // 使用操作系统shell启动进程
                //proc.StartInfo.CreateNoWindow = true;
                //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//这里设置DOS窗口不显示，经实践可行
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }

        }
        public string ToEncrypt(string encryptKey, string str)
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

        public string ToDecrypt(string encryptKey, string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                    return null;
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
