using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace desktop
{
    public partial class 分步解析 : Form
    {
        string text;
        int step = 1;

        class info
        {
            public string meaning;
            public string color;

            public info(string mean,string color)
            {
                this.meaning = mean;
                this.color = color;
            }
        }
        //存储颜色
        Dictionary<string, info> postages = new Dictionary<string, info>
        {
            {"a",new info("形容词","#4F4F4F") },
            {"b",new info("其他的修饰名词","#750000") },
            {"c",new info("连词","#BF0060") },
            {"d",new info("副词","#930093") },
            {"e",new info("感叹词","#5B00AE") },
            {"g",new info("词素","#0000C6") },
            {"h",new info("前缀","#005AB5") },
            {"i",new info("成语","#009393") },
            {"j",new info("缩写","#01B468") },
            {"k",new info("后缀","#00A600") },
            {"m",new info("数字","#73BF00") },
            {"n",new info("一般名词","#8C8C00") },
            {"nd",new info("方向名词","#AE8F00") },
            {"nh",new info("人名","#D26900") },
            {"ni",new info("公司名","#BB3D00") },
            {"nl",new info("地点名词","#984B4B") },
            {"ns",new info("地理名词","#949449") },
            {"nt",new info("时间名词","#4F9D9D") },
            {"nz",new info("其他名词","#7373B9") },
            {"o",new info("拟声词","#9F4D95") },
            {"p",new info("介词","#EA0000") },
            {"q",new info("量词","#FF359A") },
            {"r",new info("代词","#FF00FF") },
            {"u",new info("助词","#9F35FF") },
            {"v",new info("动词","#6A6AFF") },
            {"wp",new info("标点","#2894FF") },
            {"ws",new info("国外词","#00FFFF") },
            {"x",new info("不构成词","#1AFD9C") }
        };

        public 分步解析(string text)
        {
            InitializeComponent();
            this.text = text;
        }

        private void 分句()
        {
            if (Data.cut_sentence_result.potentials != null)
            {
                Data.cut_sentence_result.potentials.Clear();
            }
            string jsonstr = Web.Post(text, Data.config["get_sentences"]);
            if (jsonstr != "error")
            {
                Data.cut_sentence_result = JsonConvert.DeserializeObject<Data.Cut_sentence_result>(jsonstr);
                panel_load(Data.cut_sentence_result);
                tabControl1.SelectedIndex = 0;
            }
        }

        private void 分词()
        {
            Data.cut_word_result.Clear();
            if (Data.cut_sentence_result.potentials == null)
            {
                MessageBox.Show("请先分句");
            }
            else
            {
                foreach (var potential in Data.cut_sentence_result.potentials)
                {
                    foreach (var command in potential)
                    {
                        foreach (var sentence in command)
                        {
                            string jsonstr = Web.Post(sentence.sen, Data.config["segment"]);
                            if (jsonstr != "error")
                            {
                                Data.cut_word_result.Add(JsonConvert.DeserializeObject<Data.Cut_word_result>(jsonstr));
                            }
                            else
                            {
                                Data.cut_word_result.Clear();
                                break;
                            }
                        }
                    }
                }
                panel_load(Data.cut_word_result);
                tabControl1.SelectedIndex = 1;
            }
        }

        private void 词性分析()
        {
            Data.part_of_speech_analysis_results.Clear();
            if (Data.cut_word_result.Count == 0)
            {
                MessageBox.Show("输入为空");
            }
            else
            {
                foreach (var words in Data.cut_word_result)
                {
                    string jsonstr = Web.Post(JsonConvert.SerializeObject(words),Data.config["postag"]);
                    if (jsonstr != "error")
                    {
                        Data.part_of_speech_analysis_results.Add(JsonConvert.DeserializeObject<Data.Part_of_speech_analysis_result>(jsonstr));
                    }
                    else
                    {
                        Data.part_of_speech_analysis_results.Clear();
                        break;
                    }
                }
                panel_load(Data.part_of_speech_analysis_results);
                tabControl1.SelectedIndex = 2;
            }
        }

        private void 依存句法分析()
        {
            Data.syntactic_analysis_results.Clear();
            int length = Data.cut_word_result.Count;
            if (length == 0)
            {
                MessageBox.Show("输入为空");
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    string data = JsonConvert.SerializeObject(Data.cut_word_result[i]) + "&" + JsonConvert.SerializeObject(Data.part_of_speech_analysis_results[i]);
                    string jsonstr = Web.Post(data,Data.config["parser"]);
                    if (jsonstr != "error")
                    {
                        Data.syntactic_analysis_results.Add(JsonConvert.DeserializeObject<Data.Syntactic_analysis_result>(jsonstr));
                    }
                    else
                    {
                        Data.syntactic_analysis_results.Clear();
                        break;
                    }
                }
                panel_load(Data.syntactic_analysis_results);
                tabControl1.SelectedIndex = 3;
            }
        }

        private void 主成分提取()
        {
            int number = 0;
            Data.sen_res.Clear();
            if (Data.cut_sentence_result.potentials.Count == 0)
            {
                MessageBox.Show("输入为空");
            }
            else
            {
                for (int i = 0; i < Data.cut_sentence_result.potentials.Count; i++)
                {
                    for (int j = 0; j < Data.cut_sentence_result.potentials[i].Count; j++)
                    {
                        for (int k = 0; k < Data.cut_sentence_result.potentials[i][j].Count; k++)
                        {
                            Data.cut_sentence_result.potentials[i][j][k].sen_res_poi = number;
                            string data = JsonConvert.SerializeObject(Data.syntactic_analysis_results[number]);
                            number++;
                            string jsonstr = Web.Post(data,Data.config["main_component"]);
                            if (jsonstr != "error")
                            {
                                Data.Main_component_result main_Component_Result = JsonConvert.DeserializeObject<Data.Main_component_result>(jsonstr);
                                Data.sen_res.Add(new Data.Sen_res(i, j, k, main_Component_Result.main_component));
                            }
                            else
                            {
                                Data.sen_res.Clear();
                                break;
                            }
                        }
                    }
                }
                panel_load(Data.sen_res);
                tabControl1.SelectedIndex = 4;
            }
        }

        //更改下一步执行的函数
        private void button1_Click(object sender, EventArgs e)
        {
            switch (step)
            {
                case 1:
                    分句();
                    break;
                case 2:
                    分词();
                    break;
                case 3:
                    词性分析();
                    break;
                case 4:
                    依存句法分析();
                    break;
                case 5:
                    主成分提取();
                    button1.Enabled = false;
                    button2.Text = "关闭";
                    break;
                default:
                    break;
            }
            step++;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (step == 6)
            {
                if(MessageBox.Show("是否保存结果？","结果保存", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) == DialogResult.Yes)
                {
                    SaveFileDialog saveFile = new SaveFileDialog();
                    saveFile.Title = "请选择保存文件路径";
                    saveFile.Filter = "JSON文件(*.json)|*.json";
                    if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Data.save_main_data(saveFile.FileName);
                    }
                }
            }
            this.Close();
        }

        //几处重载，用于显示返回的数据
        private void panel_load(Data.Cut_sentence_result result)
        {
            int height = 0;
            foreach(var potential in result.potentials)
            {
                foreach(var command in potential)
                {
                    foreach(var sen in command)
                    {
                        Label label = new Label();
                        label.Text = sen.sen;
                        label.Location = new Point(0, height);
                        label.AutoSize = true;
                        height += label.Height;
                        tabPage1.Controls.Add(label);
                    }
                }
            }
        }

        private void panel_load(List<Data.Cut_word_result> result)
        {
            int num = 0;
            foreach(var sen in result)
            {
                string new_sen = "";
                foreach(var verb in sen.words)
                {
                    new_sen += verb + " ";
                }
                new_sen=new_sen.Remove(new_sen.Length - 1);
                TextBox textBox = new TextBox();
                textBox.Text = new_sen;
                textBox.Width = panel1.Width - 18;
                textBox.Margin = new Padding(0);
                textBox.Tag = num;
                textBox.Location = new Point(4, num++ *(textBox.Height+6));
                textBox.TextChanged += TextBox_TextChanged;
                tabPage2.Controls.Add(textBox);
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            string[] words = textbox.Text.Split(' ');
            Data.cut_word_result[(int)textbox.Tag].words.Clear();
            foreach(var word in words)
            {
                Data.cut_word_result[(int)textbox.Tag].words.Add(word);
            }
        }

        private void panel_load(List<Data.Part_of_speech_analysis_result> result)
        {
            int height = 0;
            int temp_height = 0;
            for(int i=0;i<result.Count;i++)
            {
                int width = 0;
                for (int j = 0; j < result[i].postags.Count; j++)
                {
                    string innner_text = Data.cut_word_result[i].words[j] + "(" + result[i].postags[j] + ")";
                    Label label = new Label();
                    label.Text = innner_text;
                    label.ForeColor = ColorTranslator.FromHtml(postages[result[i].postags[j]].color);
                    label.AutoSize = true;
                    label.Location = new Point(width, height);
                    Graphics g = label.CreateGraphics();
                    SizeF StrSize = g.MeasureString(label.Text, label.Font);
                    width += ((int)StrSize.Width + 50);
                    tabPage3.Controls.Add(label);
                    temp_height = label.Height;
                }
                height += temp_height;
            }
        }

        private void panel_load(List<Data.Syntactic_analysis_result> result)
        {
            for(int i=0;i<result.Count;i++)
            {
                TreeNode root_node = new TreeNode();
                root_node.Text = "第" + (i+1).ToString() + "句:";
                foreach(var item in result[i].relation)
                {
                    TreeNode node = new TreeNode();
                    node.Text = "(" + item[0] + "," + item[1] + "," + item[2] + ")";
                    root_node.Nodes.Add(node);
                }
                treeView1.Nodes.Add(root_node);
            }
            treeView1.Nodes[0].Expand();
        }

        private void panel_load(List<Data.Sen_res> result)
        {
            for (int i = 0; i < result.Count; i++)
            {
                TreeNode root_node = new TreeNode();
                root_node.Text = "第" + (i+1).ToString() + "句:";
                foreach (var item in result[i].remake_res)
                {
                    TreeNode node = new TreeNode();
                    node.Text = "(" + item[0] + "," + item[1] + "," + item[2] + ")";
                    root_node.Nodes.Add(node);
                }
                treeView2.Nodes.Add(root_node);
            }
            treeView2.Nodes[0].Expand();
        }
    }
}
