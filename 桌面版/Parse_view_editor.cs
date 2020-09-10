using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace desktop
{
    class arse_view_editor
    {
        private static int sen_id = 0;
        public static void Set_sentences(Panel panel,Data.Cut_sentence_result cut)
        {
            int number = 0;
            foreach (var potential in cut.potentials)
            {
                foreach(var command in potential)
                {
                    foreach(var sentence in command)
                    {
                        Button button = new Button();
                        button.Text = sentence.sen;
                        button.TextAlign = ContentAlignment.MiddleLeft;
                        button.Tag = number;
                        button.Width = panel.Width;
                        button.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                        button.Location = new Point(0, number*button.Height);
                        button.Paint += button_Paint;
                        panel.Controls.Add(button);
                        number++;
                    }
                }
            }
        }

        public static void Set_buttons(Panel panel, Panel panel2)
        {
            foreach(Control control in panel.Controls)
            {
                if(control is Button)
                {
                    control.Click += (sender, e) => button_click(sender,e, panel2);
                }
            }
        }

        private static void button_click(object sender, EventArgs e,Panel panel)
        {
            Button button = (Button)sender;
            sen_id = (int)button.Tag;
            set_contains(panel);
        }

        private static void button_Paint(object sender, PaintEventArgs e)
        {
            Button button = (Button)sender;
            e.Graphics.DrawLine(Pens.Black, 0, button.Height - 1, button.Width - 1, button.Height - 1);//下边线
        }

        private static void set_contains(Panel panel)
        {
            panel.Controls.Clear();
            for (int i = 0; i < Data.sen_res[sen_id].remake_res.Count; i++)
            {
                for (int j = 0; j < Data.sen_res[sen_id].remake_res[i].Count; j++)
                {
                    TextBox textBox = new TextBox();
                    textBox.Tag = new int[] { i, j };
                    textBox.Text = Data.sen_res[sen_id].remake_res[i][j];
                    textBox.Width = panel.Width / 3;
                    textBox.Location = new Point(j * textBox.Width, i * textBox.Height);
                    textBox.TextChanged +=TextBox_TextChanged;
                    panel.Controls.Add(textBox);
                }
            }
        }

        private static void TextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            int[] tag = (int[])textBox.Tag;
            Data.sen_res[sen_id].remake_res[tag[0]][tag[1]] = textBox.Text;
        }

        public static void button1_Click(object sender, EventArgs e,Panel panel)
        {
            Data.sen_res[sen_id].remake_res.Add(new List<string> { "", "", "" });
            for (int i = 0; i < 3; i++)
            {
                TextBox textBox = new TextBox();
                textBox.Tag = new int[] { Data.sen_res[sen_id].remake_res.Count - 1, i };
                textBox.Text = Data.sen_res[sen_id].remake_res.Last()[i];
                textBox.Width = panel.Width / 3;
                textBox.Location = new Point(i * textBox.Width, (Data.sen_res[sen_id].remake_res.Count - 1) * textBox.Height);
                textBox.TextChanged += TextBox_TextChanged;
                panel.Controls.Add(textBox);
            }
        }

        public static void button2_Click(object sender, EventArgs e,Panel panel)
        {
            for (int i = 0; i < 3; i++)
            {
                if(panel.Controls.Count>0) panel.Controls.RemoveAt(panel.Controls.Count - 1);
            }
            if(Data.sen_res[sen_id].remake_res.Count>0) Data.sen_res[sen_id].remake_res.RemoveAt(Data.sen_res[sen_id].remake_res.Count - 1);
        }
    }
}
