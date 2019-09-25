using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HansLinkManage.CommonClass
{
    public class MasMXG
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MXGRecData
        {
            public ushort m_nNetID;              //网络号
            public ushort m_nIndex;             //测试点编号
            public Point m_Pin;
            public Point m_Grid;
            public ushort m_XSize;
            public ushort m_YSize;
            public ushort m_nUnitID;             //单元号
            [MarshalAs(UnmanagedType.I1)]
            public bool m_IsCSide;
        }
        public static string MxgName = "";
        public static string barDirpath = "";
        private string ErpFile;
        List<MXGRecData> lMRd;
        public Dictionary<int, List<KeyValuePair<MXGRecData, MXGRecData>>> DErpTab;
        public Dictionary<int, List<KeyValuePair<MXGRecData, MXGRecData>>>.KeyCollection keyCol;

        int RoundTo(double dVal)
        {
            if (dVal < 0)
            {
                return (int)(dVal - 0.5);
            }
            else
            {
                return (int)(dVal + 0.5);
            }
        }

        double CalcMP(double nMP)
        {
            int MP = RoundTo(nMP / 39.37);
            nMP = MP * 40;
            return nMP;
        }

        public MasMXG()
        {
            lMRd = new List<MXGRecData>();
            DErpTab = new Dictionary<int, List<KeyValuePair<MXGRecData, MXGRecData>>>();
        }

        public bool Import(string szFileName)
        {
            if (String.IsNullOrEmpty(szFileName))
                return false;
            lMRd.Clear();
            try
            {
                StreamReader sr = new StreamReader(szFileName);
                while (sr.Peek() > -1)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    if (line[0] <= '9' && line[0] >= '0')
                    {
                        ProcessMxgData(line);
                    }
                }
                sr.Close();

            }
            catch (Exception ce)
            {
                throw new Exception(ce.Message);
            }
            return true;
        }

        private void ProcessMxgData(string szline)
        {
            MXGRecData rd = new MXGRecData();
            string[] str = SplitString(szline, ",");
            rd.m_nIndex = ushort.Parse(str[0]);
            rd.m_Pin.X = ExtractNum(str[1].Substring(1, 5))*10;
            rd.m_Pin.Y= ExtractNum(str[1].Substring(7, 5))*10;
            rd.m_Grid.X= (int)CalcMP((double)(ExtractNum(str[2].Substring(1, 5))));
            rd.m_Grid.Y= (int)CalcMP((double)(ExtractNum(str[2].Substring(7, 5))));
            rd.m_IsCSide = (str[3][1] == '0');
            rd.m_nUnitID = ExtractNum_u16(str[4]);
            rd.m_nNetID = ExtractNum_u16(str[8]);
            rd.m_XSize = (ushort)(ExtractNum_u16(str[11])*10);
            rd.m_YSize = (ushort)(ExtractNum_u16(str[12])*10);
            lMRd.Add(rd);
        }

        public bool ImportErp(string szFileName)
        {
            if (String.IsNullOrEmpty(szFileName))
                return false;
            DErpTab.Clear();
            try
            {
                StreamReader sr = new StreamReader(szFileName);
                while (sr.Peek() > -1)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    if (!ProcessErpData(line))
                        return false;
                }
                sr.Close();

            }
            catch (Exception ce)
            {
                throw new Exception(ce.Message);
            }
            return true;
        }

        private bool ProcessErpData(string szline)
        {
            List<KeyValuePair<MXGRecData, MXGRecData>> Erplist = new List<KeyValuePair<MXGRecData, MXGRecData>>();
            string[] str = SplitString(szline, "\\|");
            MxgName = str[0];
            int Phao = ExtractNum(str[1]);
            int huaibanhao= ExtractNum(str[2]);
            int index = Phao * 10000 + huaibanhao;
            for (int i=4;i< str.Length;i++)
            {               
                  string oors = str[i].Substring(0, 1);
                if (oors != "O")
                    break; 
                int ax = ExtractNum(str[i].Substring(3, 6));
                int ay = ExtractNum(str[i].Substring(11, 6));
                string aCS = str[i].Substring(19, 1);
                int bx = ExtractNum(str[i].Substring(23, 6));
                int by = ExtractNum(str[i].Substring(31, 6));
                string bCS = str[i].Substring(39, 1);
                Point apoint = new Point(ax, ay);
                Point bpoint = new Point(bx, by);
                MXGRecData aMxgRd = new MXGRecData();
                MXGRecData bMxgRd = new MXGRecData();
                if(!GetMxgRdInfo(apoint, (aCS == "B"), ref aMxgRd))
                {
                    MessageBox.Show("erp文件解析错误");
                    return false;
                }
                if(!GetMxgRdInfo(bpoint, (bCS == "B"), ref bMxgRd))
                {
                    MessageBox.Show("erp文件解析错误");
                    return false;
                }
                KeyValuePair<MXGRecData, MXGRecData> kvpRd = new KeyValuePair<MXGRecData, MXGRecData>(aMxgRd, bMxgRd);
                Erplist.Add(kvpRd);
            }
            DErpTab[index] = Erplist;
            return true;
        }
        bool GetMxgRdInfo(Point TabPoint, bool isCSide,ref MXGRecData Mxgrd )
        {
            foreach(var vrd in lMRd)
            {
                if ((vrd.m_Grid.X == TabPoint.X) && (vrd.m_Grid.Y  == TabPoint.Y)
                    &&(vrd.m_IsCSide == isCSide))
                {
                    Mxgrd = vrd;
                    return true;
                }
            }
            return false;
        }


        public static string[] SplitString(string strContent, string strSplit)
        {
            string[] strArray = null;
            if (!string.IsNullOrEmpty(strContent))
            {
                strArray = new Regex(strSplit).Split(strContent);
            }
            return strArray;
        }

        public static string[] SplitString(string strContent, string strSplit, int p)
        {
            string[] result = new string[p];
            string[] splited = SplitString(strContent, strSplit);
            int j = 0;
            for (int i = 0; i < splited.Length; i++)
            {
                if (String.IsNullOrEmpty(splited[i]))
                    ;
                else
                {
                    result[j] = splited[i];
                    j++;
                }
            }
            return result;
        }

        public static int ExtractNum(string sourceString)
        {
            //  string result = Regex.Replace(sourceString, "[^+-][^0-9]+", "");
            string result = Regex.Replace(sourceString, "[^0-9+-]+", "");
            try
            {
                return int.Parse(result);
            }
            catch (FormatException)
            {
                return 0;
            }
        }

        public static bool QuickValidate(string _express, string _value)
        {
            if (_value == null) return false;
            Regex myRegex = new Regex(_express);
            if (_value.Length == 0)
            {
                return false;
            }
            return myRegex.IsMatch(_value);
        }
        public static bool IsNumberId(string _value)
        {
            return QuickValidate("^[1-9]*[0-9]*$", _value);
        }
        public static uint ExtractNum_u(string sourceString)
        {
            string result = Regex.Replace(sourceString, "[^0-9]+", "");
            if (IsNumberId(result))
                return uint.Parse(result);
            else
                return 0;
        }

        public static ushort ExtractNum_u16(string sourceString)
        {
            string result = Regex.Replace(sourceString, "[^0-9]+", "");
            if (IsNumberId(result))
                return ushort.Parse(result);
            else
                return 0;
        }

        public static string LeftSplit(string sourceString, char splitChar)
        {
            string result = null;
            string[] tempString = sourceString.Split(splitChar);
            if (tempString.Length > 0)
            {
                result = tempString[0].ToString();
            }
            return result;
        }
        public static string LeftSplit(string sourceString, char[] splitChar)
        {
            string result = null;
            string[] tempString = sourceString.Split(splitChar);
            if (tempString.Length > 0)
            {
                result = tempString[0].ToString();
            }
            return result;
        }

        public static string RightSplit(string sourceString, char splitChar)
        {
            string result = null;
            string[] tempString = sourceString.Split(splitChar);
            if (tempString.Length > 0)
            {
                result = tempString[tempString.Length - 1].ToString();
            }
            return result;
        }

        public void ErpReport(int _index)
        {
           if(DErpTab.Count>0)
           {
                string strtime = DateTime.Now.GetDateTimeFormats('r')[0].ToString();
                ErpFile = barDirpath + @"\" + _index.ToString().PadLeft(8, '0') + ".erp";
                if (File.Exists(ErpFile))
                {
                    MessageBox.Show(_index.ToString().PadLeft(8, '0')+".erp文件出现重复，请查看！！！");
                    return;
                }
                else
                {
                    List<KeyValuePair<MXGRecData, MXGRecData>> LErpBar = DErpTab[_index];
                    string erpRep="";
                    foreach(var kvprep in LErpBar)
                    {
                        string astrx = kvprep.Key.m_Grid.X.ToString().PadLeft(6, '0');
                        string astry = kvprep.Key.m_Grid.Y.ToString().PadLeft(6, '0');
                        string bstrx = kvprep.Value.m_Grid.X.ToString().PadLeft(6, '0');
                        string bstry = kvprep.Value.m_Grid.Y.ToString().PadLeft(6, '0');
                        erpRep += "|O " + "(" + astrx + ", " + astry + ", " + (kvprep.Key.m_IsCSide ? "B" : "T") + ") " +
                            "(" + bstrx + ", " + bstry + ", " + (kvprep.Value.m_IsCSide ? "B" : "T") + ") ";
                    }
                    string erpEnd = "  $ % ^ & *";
                    using (StreamWriter writer = File.CreateText(ErpFile))
                        writer.WriteLine(string.Format("{0}|{1}|{2}|{3}{4}{5}", MxgName, _index.ToString().PadLeft(8, '0').Substring(0,4), _index.ToString().PadLeft(8, '0').Substring(4, 4),
                            strtime, erpRep,erpEnd));
                }
                
            }   
        }

    }
}
