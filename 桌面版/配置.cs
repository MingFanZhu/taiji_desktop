using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace desktop
{
    public partial class 配置 : Form
    {
        public delegate void changed();
        public event changed change = null;

        public 配置()
        {
            InitializeComponent();
        }

        private void 配置_Load(object sender, EventArgs e)
        {
            textBox1.Text = Data.config["font_color"];
            textBox2.Text = Data.config["font_size"];
            textBox3.Text= Data.config["line_space"];
            textBox5.Text = Data.config["text_folder"];
            textBox6.Text = Data.config["json_folder"];
            textBox4.Text = Data.config["get_sentences"];
            textBox7.Text = Data.config["segment"];
            textBox8.Text = Data.config["postag"];
            textBox9.Text = Data.config["parser"];
            textBox10.Text = Data.config["main_component"];
            textBox11.Text = Data.config["upload"];
            textBox12.Text = Data.config["preview"];
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            foreach(var family in installedFontCollection.Families)
            {
                comboBox1.Items.Add(family.Name);
            }
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Data.config["font_family"]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ColorTranslator.ToHtml(colorDialog.Color);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择文本文件所在文件夹";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox5.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择JSON文件存放文件夹";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox6.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //保存并触发事件
        private void button5_Click(object sender, EventArgs e)
        {
            Data.config["font_color"] = textBox1.Text;
            Data.config["font_size"] = textBox2.Text;
            Data.config["line_space"] = textBox3.Text;
            Data.config["font_family"] = comboBox1.Text;
            Data.config["text_folder"] = textBox5.Text;
            Data.config["json_folder"] = textBox6.Text;
            Data.config["get_sentences"] =textBox4.Text ;
            Data.config["segment"] = textBox7.Text;
            Data.config["postag"] = textBox8.Text;
            Data.config["parser"] = textBox9.Text;
            Data.config["main_component"] = textBox10.Text;
            Data.config["upload"] = textBox11.Text;
            Data.config["preview"] = textBox12.Text;
            Data.save_config();
            change();
            this.Close();
        }
    }
}
