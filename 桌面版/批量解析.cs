using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace desktop
{
    public partial class 批量解析 : Form
    {
        private delegate void SetPos();

        public 批量解析(List<string>files)
        {
            InitializeComponent();
            foreach(string file in files)
            {
                comboBox1.Items.Add(file);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text += comboBox1.Text + "\r\n";
        }

        //添加文件
        private void iconButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = true;
            open.Title = "请选择文本";
            open.Filter = "文本文件(*.txt)|*.txt";
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach(string file_name in open.FileNames)
                {
                    textBox1.Text += file_name + "\r\n";
                }
            }
        }

        //删除
        private void iconButton2_Click(object sender, EventArgs e)
        {
            List<string> lines = textBox1.Lines.ToList();
            if(lines[lines.Count-1]=="")
                lines.RemoveAt(lines.Count - 1);
            lines.RemoveAt(lines.Count - 1);
            textBox1.Lines = lines.ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //选择文件夹
        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择文件夹";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog.SelectedPath;
            }
        }

        //对每个文件开辟子线程单独处理
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Lines[textBox1.Lines.Length - 1] == "")
                progressBar1.Maximum = textBox1.Lines.Length - 1;
            else
                progressBar1.Maximum = textBox1.Lines.Length;
            progressBar1.Value = 0;
            foreach (string file in textBox1.Lines)
            {
                if (file != "")
                {
                    //带参数子线程
                    Thread parameterizedThread = new Thread(new ParameterizedThreadStart(handle_file));
                    parameterizedThread.Start(file);
                }
            }
        }

        //子线程处理函数
        private void handle_file(object obj)
        {
            string file_path = (string)obj;
            string text = File.ReadAllText(file_path);
            string jsonstr = Web.Post(text, "http://localhost:5000/upload");
            if (jsonstr != "error")
            {
                Data.Once once = JsonConvert.DeserializeObject<Data.Once>(jsonstr);
                string new_file_name = Path.GetFileNameWithoutExtension(file_path);
                string new_file_path="";
                if (textBox2.Text != "")
                {
                    new_file_path = textBox2.Text + new_file_name + ".json";
                }
                else
                {
                    new_file_path = file_path.Replace(".txt", ".json");
                }
                File.WriteAllText(new_file_path, JsonConvert.SerializeObject(once));
                send_messsage();
            }
        }

        //更新进度条
        private void send_messsage()
        {
            if (this.InvokeRequired)//判断是否是控件所在线程外的线程需调用本控件
            {
                SetPos setpos = new SetPos(send_messsage);
                this.Invoke(setpos);
            }
            else
            {
                progressBar1.Value++;
                label3.Text = "已完成" + progressBar1.Value.ToString() + "/" + progressBar1.Maximum.ToString() + "：";
            }
        }
    }
}
