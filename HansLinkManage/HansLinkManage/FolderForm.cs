using FPT.CommonClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HansLinkManage
{
    public partial class FolderForm : Form
    {
        public FolderForm()
        {
            InitializeComponent();
        }

        FileOperate baseFileOperate = new FileOperate();
        private void FolderForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (String.IsNullOrEmpty(MainForm._mainForm.masonPath))
            {
                base.Close();
                Application.Exit();
            }
        }

        private void FolderForm_Load(object sender, EventArgs e)
        {
            baseFileOperate.AllPath = Application.StartupPath + "\\";
            baseFileOperate.CreateFileDir("HLMConfig", 1);
            baseFileOperate.AllPath = baseFileOperate.AllPath + "HLMConfig" + "\\";
            baseFileOperate.CreateFileDir("Mason.INI", 0);
            baseFileOperate.IniFileName = baseFileOperate.AllPath + "Mason.INI";
            FileOperate.inipath = baseFileOperate.ReadIniData("Mason", "DataPath", String.Empty);
            if (FileOperate.inipath == String.Empty)
            {
                baseFileOperate.WriteIniData("Mason", "DataPath", "D:\\");
                FileOperate.inipath = baseFileOperate.ReadIniData("Mason", "DataPath", String.Empty);
            }
            textBox1.Text = FileOperate.inipath;
            tVfolder.BeginUpdate();
            string[] strFolder = baseFileOperate.GetFolderNames(FileOperate.inipath);
            foreach (string strDir in strFolder)
            {
                string strtemp = baseFileOperate.GetDirectoryNames(strDir);
                if (strtemp == "RECYCLER" || strtemp == "RECYCLED" || strtemp == "Recycled" || strtemp == "System Volume Information" || strtemp == "$RECYCLE.BIN")
                { }
                else
                {
                    TreeNode tnMyDrives = new TreeNode(strtemp);
                    tVfolder.Nodes.Add(tnMyDrives);
                }
                tVfolder.ImageList = imageList1;
                tVfolder.ImageIndex = 0;
                tVfolder.SelectedImageIndex = 1;

            }
            tVfolder.EndUpdate();
        }

        private void btnChangeDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "请选择Mason批量号文件夹";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                tVfolder.Nodes.Clear();
                baseFileOperate.WriteIniData("Mason", "DataPath", folderBrowserDialog1.SelectedPath);
                FileOperate.inipath = baseFileOperate.ReadIniData("Mason", "DataPath", String.Empty);
                textBox1.Text = FileOperate.inipath;
                tVfolder.BeginUpdate();
                string[] strFolder = baseFileOperate.GetFolderNames(FileOperate.inipath);
                foreach (string strDir in strFolder)
                {
                    string strtemp = baseFileOperate.GetDirectoryNames(strDir);
                    if (strtemp == "RECYCLER" || strtemp == "RECYCLED" || strtemp == "Recycled" || strtemp == "System Volume Information" || strtemp == "$RECYCLE.BIN")
                    { }
                    else
                    {
                        TreeNode tnMyDrives = new TreeNode(strtemp);
                        tVfolder.Nodes.Add(tnMyDrives);
                    }
                    tVfolder.ImageList = imageList1;
                    tVfolder.ImageIndex = 0;
                    tVfolder.SelectedImageIndex = 1;

                }
                tVfolder.EndUpdate();
            }
        }

        private void tVfolder_DoubleClick(object sender, EventArgs e)
        {
            BatchForm batchForm = new BatchForm();
            MainForm._mainForm.batchForm = batchForm;
            batchForm.mFolderName = tVfolder.SelectedNode.FullPath;
            MainForm._mainForm.MaintextBox1 = FileOperate.inipath + "\\" + tVfolder.SelectedNode.FullPath;
            this.Hide();
            batchForm.ShowDialog();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tVfolder.Nodes.Count == 0)
                return;
            BatchForm batchForm = new BatchForm();
            MainForm._mainForm.batchForm = batchForm;
            batchForm.mFolderName = tVfolder.SelectedNode.FullPath;
            MainForm._mainForm.MaintextBox1 = FileOperate.inipath + "\\" + tVfolder.SelectedNode.FullPath;
            this.Hide();
            batchForm.ShowDialog();
        }
    }
}
