using System;
using FPT.CommonClass;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HansLinkManage.CommonClass;
using System.IO;

namespace HansLinkManage
{
    public delegate void ReadMxgOrErpHandler(string _path);
    public partial class FileForm : Form
    {
        public FileForm()
        {
            InitializeComponent();
        }
        MasForm masForm;
        private string MxgDirpath = "";
        private string MxgDirectory;
        public event ReadMxgOrErpHandler ReadMxgEvent, ReadErpEvent;
        FileOperate baseFileOperate = new FileOperate();
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
                return;
            if(listView1.SelectedItems.Count==2)
            {
                if (MasMXG.RightSplit(listView1.SelectedItems[0].Text,'.').ToLower()=="mxg"&& MasMXG.RightSplit(listView1.SelectedItems[1].Text, '.').ToLower() == "erp")
                {
                    string file_path = textBox1.Text + "\\" + listView1.SelectedItems[0].Text;
                    string file_path1 = textBox1.Text + "\\" + listView1.SelectedItems[1].Text;
                    ReadMxgRun(file_path);
                    ReadErpRun(file_path1);
                }
                else if (MasMXG.RightSplit(listView1.SelectedItems[1].Text, '.').ToLower() == "mxg" && MasMXG.RightSplit(listView1.SelectedItems[0].Text, '.').ToLower() == "erp")
                {
                    string file_path = textBox1.Text + "\\" + listView1.SelectedItems[1].Text;
                    string file_path1 = textBox1.Text + "\\" + listView1.SelectedItems[0].Text;
                    ReadMxgRun(file_path);
                    ReadErpRun(file_path1);
                }
                else
                {
                    MessageBox.Show("选中的文件有误！！！");
                    return;
                }
                Hide();
                CreateBarDir();
                masForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("请同时选择两个文件！！！");
            }
            
        }

        private void btnChooseDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "请选择Mason料号文件夹";
            folderBrowserDialog1.SelectedPath = FileOperate.inipath;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
                baseFileOperate.GetListViewItem(textBox1.Text, imageList1, listView1);
                MxgDirpath = textBox1.Text;
            }
        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FileForm_Load(object sender, EventArgs e)
        {
            masForm = new MasForm();
            ReadMxgEvent += new ReadMxgOrErpHandler(masForm.FilePathListView);
            ReadErpEvent += new ReadMxgOrErpHandler(masForm.FilePathListView1);
        }

        private void CreateBarDir()
        {
            int nuM = 1;
            nuMnext:
            MxgDirectory = MxgDirpath + @"\" + nuM.ToString().PadLeft(6, '0');
            MasMXG.barDirpath = MxgDirectory;
            if (!Directory.Exists(MxgDirectory))
            {
                Directory.CreateDirectory(MxgDirectory);
            }
            else
            {
                nuM++;
                goto nuMnext;
            }
        }

        private void ReadMxgRun(string str)
        {
            if (ReadMxgEvent != null)
            {
                ReadMxgEvent(str);
            }
        }

        private void ReadErpRun(string str)
        {
            if (ReadErpEvent != null)
            {
                ReadErpEvent(str);
            }
        }
    }
}
