using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using UartUpdata;

namespace AutoChart
{
    public partial class form1 : Form
    {

        SerialPort sp = new SerialPort();
        byte[] uartRxBuffer = new byte[1024];

        byte[] Config_cmd = new byte[100];

        DataTable InverterStatusDataTable = new DataTable();

        public form1()
        {
            InitializeComponent();


            #region 设备路由表

            #endregion
        }

        // send data by uart

        private delegate void SetTextUnSafe(Control control, string strText);

        private void SetText(Control control, string strText)
        {
            if (this.InvokeRequired)
            {
                SetTextUnSafe InvokeSetText = new SetTextUnSafe(SetText);
                this.Invoke(InvokeSetText, new object[] { control, strText });
            }
            else
            {
                control.Text = strText;
            }
        }

        private void AddText(TextBox control, string strText)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    int t = control.SelectionStart;
                    if (t == control.TextLength)
                    {
                        control.AppendText(strText + "\r\n");
                    }
                    else
                    {
                        control.AppendText(strText + "\r\n");
                        control.SelectionStart = t;
                        control.ScrollToCaret();
                    }
                }));
            }
            else
            {
                int t = control.SelectionStart;
                if (t == control.TextLength)
                {
                    control.AppendText(strText + "\r\n");
                }
                else
                {
                    control.AppendText(strText + "\r\n");
                    control.SelectionStart = t;
                    control.ScrollToCaret();
                }
            }
        }

        private void form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }


        private void putlog(string message)
        {
            AddText(textBox1,message);
        }

        int diff_time = 0;
        int TIME_DIFF = 50;
        string path = string.Empty;

        private void button_select_file_path(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Files (*.txt)|*.txt"  //如果需要筛选txt文件（"Files (*.txt)|*.txt"）
            };

            //var result = openFileDialog.ShowDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = openFileDialog.FileName;
            }

            this.textBox_Path.Text = path;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TIME_DIFF = Convert.ToInt32(this.textBox3.Text);
            Debug.Print("超时： " + TIME_DIFF + " ms");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filter = this.textBox2.Text;
            if(filter == String.Empty || path == String.Empty)
            {
                MessageBox.Show("未设置过滤数据，未选择文件！！！！");
                return;
            }
            string[] linestr = File.ReadAllLines(path);
            Debug.Print("该文件一共有: linestr.Length :\t" + linestr.Length + "行");
            putlog("该文件一共有" + linestr.Length + "行");

            string str = linestr[0];
            string time_str = str.Substring(1, 12);

            DateTime preTime = DateTime.ParseExact(time_str, "HH:mm:ss.fff", System.Globalization.CultureInfo.CurrentCulture);
            DateTime nextTime;

            // 让过第一行，从第二行开始处理
            for (int i = 0; i < linestr.Length; i++)
            {
                str = linestr[i];
                if (str.Length > 20)
                {
                    if (str.Contains(filter))
                    {
                        //Debug.Print(str);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    continue;
                }
                //Debug.Print("匹配成功！！！ line: " + (i+1) );

                time_str = str.Substring(1, 12);
                nextTime = DateTime.ParseExact(time_str, "HH:mm:ss.fff", System.Globalization.CultureInfo.CurrentCulture);

                if ((int)(nextTime - preTime).TotalMilliseconds > TIME_DIFF)
                {
                    diff_time = (int)(nextTime - preTime).TotalMilliseconds;

                    Debug.Print("lines:\t" + (i + 1) + "\t\t : " + "time diff > " + diff_time + ";\t" + str);
                    putlog("line: " + (i+1) + "\t\t preTime: " + preTime.ToString("mm:ss.fff") + "\tnextTime: " + nextTime.ToString("mm:ss.fff") +
                        "\t time-diff: " + diff_time + " > " + TIME_DIFF + " ms");
                }
                preTime = nextTime;
            }
        }
    }
}


