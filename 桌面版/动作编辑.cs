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
    public partial class 动作编辑 : Form
    {
        int sen_id = 0;
        int last_sen_id = 0;
        List<Panel> panels = new List<Panel>();
        string file_path = "";
        public 动作编辑()
        {
            InitializeComponent();
        }

        private void 动作编辑_Load(object sender, EventArgs e)
        {
            if (Data.cut_sentence_result.potentials != null && Data.sen_res != null)
            {
                Set_sentences();
                set_contains();
            }
            else
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                MessageBox.Show("无动作解析结果");
            }
        }

        //左侧按钮
        private void Set_sentences()
        {
            int number = 0;
            foreach (var potential in Data.cut_sentence_result.potentials)
            {
                foreach (var command in potential)
                {
                    foreach (var sentence in command)
                    {
                        Button button = new Button();
                        button.Text = sentence.sen;
                        button.TextAlign = ContentAlignment.MiddleLeft;
                        button.Tag = number;
                        button.Width = panel1.Width;
                        button.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                        button.Location = new Point(0, number * button.Height);
                        button.Paint += button_Paint;
                        button.Click += button_Click;
                        panel1.Controls.Add(button);
                        number++;
                    }
                }
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            last_sen_id = sen_id;
            sen_id = (int)button.Tag;
            panels[last_sen_id].BackColor = panels[sen_id].BackColor;
            panels[sen_id].BackColor = Color.LightBlue;
        }

        private void button_Paint(object sender, PaintEventArgs e)
        {
            Button button = (Button)sender;
            e.Graphics.DrawLine(Pens.Black, 0, button.Height - 1, button.Width - 1, button.Height - 1);//下边线
        }

        //右侧文本框
        private void set_contains()
        {
            panel2.Controls.Clear();
            panels.Clear();
            Panel panel_head = new Panel();
            panel_head.Width = panel2.ClientSize.Width - 25;
            panel_head.Margin = new Padding(0);
            panel_head.Padding = new Padding(3);
            panel_head.AutoSize = true;
            TextBox first = new TextBox();
            TextBox second = new TextBox();
            TextBox third = new TextBox();
            first.Text = "部位";
            second.Text = "方向";
            third.Text = "范围";
            first.Width= (panel_head.Width - 6) / 3;
            second.Width = first.Width;
            third.Width = second.Width;
            first.Location = new Point(3, 3);
            second.Location = new Point(3 + first.Width, 3);
            third.Location = new Point(3 + 2 * first.Width, 3);
            first.ReadOnly = true;
            second.ReadOnly = true;
            third.ReadOnly = true;
            panel_head.Controls.Add(first);
            panel_head.Controls.Add(second);
            panel_head.Controls.Add(third);
            panel_head.Height = first.Height;
            panel_head.BackColor = Color.LightGray;
            panel_head.Location = new Point(0, 0);
            panel2.Controls.Add(panel_head);
            int height = panel_head.Height;
            for (int k = 0; k < Data.sen_res.Count; k++)
            {
                Panel panel = new Panel();
                panel.Width = panel2.ClientSize.Width - 25;
                panel.AutoSize = true;
                panel.Margin = new Padding(0);
                panel.Padding = new Padding(3);
                int panel_height = 0;
                for (int i = 0; i < Data.sen_res[k].remake_res.Count; i++)
                {
                    for (int j = 0; j < Data.sen_res[k].remake_res[i].Count; j++)
                    {
                        TextBox textBox = new TextBox();
                        textBox.Tag = new int[] {k, i, j };
                        textBox.Text = Data.sen_res[k].remake_res[i][j];
                        textBox.Width = (panel.Width - 6) / 3;
                        if (i == 0) textBox.Location = new Point(j * textBox.Width + 3, i * textBox.Height + 3);
                        else textBox.Location = new Point(j * textBox.Width + 3, i * textBox.Height);
                        textBox.TextChanged += TextBox_TextChanged;
                        panel.Controls.Add(textBox);
                        if (j == 0) panel_height += textBox.Height;
                    }
                }
                panel.Height = panel_height;
                panel.Location = new Point(0, height);
                height += panel_height + 9;
                panel2.Controls.Add(panel);
                panels.Add(panel);
            }
            panels[sen_id].BackColor = Color.LightBlue;
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            int[] tag = (int[])textBox.Tag;
            Data.sen_res[tag[0]].remake_res[tag[1]][tag[2]] = textBox.Text;
        }

        //添加
        private void button1_Click(object sender, EventArgs e)
        {
            button3_Click(sender, e);
            Data.sen_res[sen_id].remake_res.Add(new List<string> { "", "", "" });
            set_contains();
        }

        //移除
        private void button2_Click(object sender, EventArgs e)
        {
            button3_Click(sender, e);
            if (Data.sen_res[sen_id].remake_res.Count > 0) Data.sen_res[sen_id].remake_res.RemoveAt(Data.sen_res[sen_id].remake_res.Count - 1);
            set_contains();
        }

        //保存
        private void button3_Click(object sender, EventArgs e)
        {
            foreach(var panel in panels)
            {
                foreach(var control in panel.Controls)
                {
                    if(control is TextBox)
                    {
                        TextBox textBox = (TextBox)control;
                        int[] tag = (int[])textBox.Tag;
                        Data.sen_res[tag[0]].remake_res[tag[1]][tag[2]] = textBox.Text;
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //打开文件
        private void 文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = false;
            open.Title = "请选择文本";
            open.Filter = "JSON文件(*.json)|*.json";
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                file_path = open.FileName;
                Data.load_main_data(file_path);
                Set_sentences();
                set_contains();
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
            }
        }

        private void 另存文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Title = "请选择保存文件路径";
            saveFile.Filter = "JSON文件(*.json)|*.json";
            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                file_path = saveFile.FileName;
                Data.save_main_data(saveFile.FileName);
            }
        }

        private void 保存文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (file_path == "")
            {
                另存文件ToolStripMenuItem_Click(sender, e);
            }else
            {
                Data.save_main_data(file_path);
            }
        }
    }
}
