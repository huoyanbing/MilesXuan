using HansLinkManage.CommonClass;
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
    public partial class MasForm : Form
    {
        public MasForm()
        {
            InitializeComponent();
        }
        MasMXG masMXG = new MasMXG();
        public void FilePathListView(string path)
        {
            OpenMasMxg(path);
        }

        public void FilePathListView1(string path)
        {
            OpenMasErp(path);
        }

        private void OpenMasMxg(string filename)
        {           
            if(!masMXG.Import(filename))
                MessageBox.Show("MXG文件解析错误！！！");
        }

        private void OpenMasErp(string filename)
        {
            if (!masMXG.ImportErp(filename))
                MessageBox.Show("erp文件解析错误！！！");
        }

        private void MasForm_Load(object sender, EventArgs e)
        {
            if(masMXG.DErpTab.Count>0)
            {
                label2.Text = MasMXG.MxgName;
                listView1.BeginUpdate();
                masMXG.keyCol = masMXG.DErpTab.Keys;
                int i = 1;
                foreach (int key in masMXG.keyCol)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = i.ToString();
                    lvi.SubItems.Add(key.ToString().PadLeft(8,'0'));
                    listView1.Items.Add(lvi);
                    i++;
                }
                listView1.EndUpdate();
                textBox1.Text = listView1.Items.Count.ToString();
            }
        }

        private void textBoxBar_TextChanged(object sender, EventArgs e)
        {
            string strBar = textBoxBar.Text;
            if(strBar.Contains("\r\n")&& strBar.Length%10==0)
            {
                string[] str = MasMXG.SplitString(strBar, "\r\n");
                int nbar = MasMXG.ExtractNum(str[str.Length - 2]);
                bool blerp = false;
                for (int i=0;i<listView1.Items.Count;i++)
                {
                    if(str[str.Length - 2]== listView1.Items[i].SubItems[1].Text)
                    {
                        
                        listView2.BeginUpdate();
                        ListViewItem lvi = new ListViewItem();
                        lvi = listView1.Items[i];
                        listView2.Items.Add((ListViewItem)lvi.Clone());
                        listView2.EndUpdate();
                        listView1.Items.RemoveAt(i);
                        blerp = true;
                        textBox1.Text = listView1.Items.Count.ToString();
                        textBox2.Text = listView2.Items.Count.ToString();
                        masMXG.ErpReport(nbar);
                    }
                }
                if(!blerp)
                {
                    MessageBox.Show("条形码重复，重新扫描！！！");
                    /*
                    textBoxBar.Clear();
                    for(int nstep=0; nstep< str.Length-1; nstep++)
                    {
                        textBoxBar.AppendText(str[nstep]+" ");
                        textBoxBar.AppendText("\r\n");
                    }*/


                }
                textBox3.Text = (str.Length-1).ToString();
            }
            


        }

        private void textBoxBar_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == (int)(char)Keys.Back)
            {
                e.Handled = true;
            }
            if (((int)e.KeyChar >= 48 && (int)e.KeyChar <= 57) || (int)e.KeyChar == (int)(char)Keys.Enter || (int)e.KeyChar == (int)(char)Keys.Tab)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
