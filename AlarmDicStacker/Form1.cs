using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: InternalsVisibleTo("AlarmDicStackerUnitTest")]

namespace AlarmDicStacker
{
    public partial class Form1 : Form
    {
        AlarmDicStacker ads;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ads = new AlarmDicStacker(openFileDialog.FileName);

                foreach ( var line in ads.Fields ){
                    dataGridView.Rows.Add(line);
                }
            }
            else
            {
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //foreach (var row in dataGridView.Rows.Cast<DataGridViewRow>())
                //{
                //    foreach (var cell in row.Cells.Cast<DataGridViewCell>())
                //    {
                //        Debug.WriteLine(cell.Value);
                //    }
                //}

                string[] subScreens = ads.GetSubData();
                for (int i=0; i<subScreens.Length; i++){
                    File.WriteAllText(folderBrowserDialog.SelectedPath+string.Format(@"\SubData{0}.csv", AlarmDicStacker.PAGE_OFFSET+i),
                        subScreens[i], Encoding.GetEncoding(932));
                }
            }
        }
    }

    public class AlarmDicStacker
    {
        public static readonly int PAGE_OFFSET = 100;
        static readonly int WIDTH = 1000;
        static readonly int HEIGHT = 1000;
        static readonly int W_OFFSET = 200;
        static readonly int H_OFFSET = 25;

        List<string[]> fields = new List<string[]>();

        public string[][] Fields { get { return fields.ToArray(); } }
        public AlarmDicStacker(string dic)
        {
            using (TextFieldParser tfp = new TextFieldParser(dic, Encoding.GetEncoding(932)))
            {
                tfp.Delimiters = new string[] { "," };
                int step = 0;
                while (tfp.EndOfData == false && step < 2)
                {
                    string[] line = tfp.ReadFields();
                    if (line[0] == "[アラーム用辞書]")
                    {
                        step++;
                    }
                    else if (line[0] == "[END]") 
                    {
                        step++;
                    } else {
                        fields.Add(line);
                    }
                }
            }
        }

        public string[] GetSubData()
        {
            int pageNo = 0;
            List<string> results = new List<string>();
            StringBuilder result = new StringBuilder();

            SetHeader(pageNo, result);
            int h_move = H_OFFSET;
            int w_move = W_OFFSET;
            for (int i = 0; i < Fields.Length; i++)
            {
                if (h_move >= HEIGHT)
                {
                    h_move = H_OFFSET;
                    w_move += W_OFFSET;
                    if (w_move >= WIDTH)
                    {
                        w_move = W_OFFSET;
                        results.Add(result.ToString());
                        result.Clear();
                        pageNo++;
                        SetHeader(pageNo, result);
                    }
                }

                result.Append(string.Format(@"STR,{0},{1},0,0,255,,5,16777215,16777215,1,{2},", 
                    w_move, h_move, Fields[i][2]));
                result.Append(@"Reserved1,,Reserved2,Reserved3,,-16,0,0,0,400,0,0,0,128,3,2,1,17,ＭＳ 明朝,0,16777215,16777215,0,単位,20,");
                result.Append(string.Format(@"0,0,0,0,0,0,0,128,0,0,0,0,ＭＳ 明朝,if([{0}]){1}", Fields[i][1], "{COLORCHANGE()},END,0"));
                result.AppendLine();

                h_move += H_OFFSET;
            }

            if (result.ToString() != "")
            {
                results.Add(result.ToString());
            }

            return results.ToArray();
        }

        private static void SetHeader(int pageNo, StringBuilder result)
        {
            result.AppendLine(string.Format(@"Page,{0},サブスクリーン {0},0,,1000,1000,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
                pageNo + PAGE_OFFSET));
        }
    }
}
