using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace desktop
{
    public partial class 查找 : Form
    {
        private int search_poi = 0;
        private RichTextBox rich;
        public 查找(RichTextBox rich_text_box)
        {
            InitializeComponent();
            this.rich = rich_text_box;
            if (rich_text_box.SelectedText != "")
            {
                textBox1.Text = rich_text_box.SelectedText;
            }
        }

        //查找字符
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                return;
            }
            int search_result = rich.Find(textBox1.Text, search_poi, RichTextBoxFinds.None);
            if (search_result == -1)
            {
                if (search_poi == 0)
                {
                    MessageBox.Show("无匹配字符！");
                }
                if (search_poi > 0)
                {
                    MessageBox.Show("已查找至末尾，无匹配字符！");
                    search_poi = 0;
                }
            }
            else
            {
                rich.Focus();
                rich.Select(search_result, textBox1.Text.Length);
                search_poi = search_result + textBox1.Text.Length;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
