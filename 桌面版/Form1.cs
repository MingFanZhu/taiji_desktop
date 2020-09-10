using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace desktop
{
    public partial class Form1 : Form
    {
        //本窗口内的全局变量
        string text = "";
        string file_path = "";
        Point mouse_position;

        //windows窗口边界编号
        const int HTLEFT = 10;
        const int HTRIGHT = 11;
        const int HTTOP = 12;
        const int HTTOPLEFT = 13;
        const int HTTOPRIGHT = 14;
        const int HTBOTTOM = 15;
        const int HTBOTTOMLEFT = 0x10;
        const int HTBOTTOMRIGHT = 17;

        //重载消息处理函数，完成无边框窗口的缩放
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x0084:
                    base.WndProc(ref m);
                    Point vPoint = new Point((int)m.LParam & 0xFFFF,(int)m.LParam >> 16 & 0xFFFF);
                    vPoint = PointToClient(vPoint);
                    if (vPoint.X <= 5)
                        if (vPoint.Y <= 5)
                            m.Result = (IntPtr)HTTOPLEFT;
                    else if(vPoint.Y >= ClientSize.Height - 5)
                            m.Result = (IntPtr)HTBOTTOMLEFT;
              else m.Result = (IntPtr)HTLEFT;
                    else if(vPoint.X >= ClientSize.Width - 5)
                        if (vPoint.Y <= 5)
                            m.Result = (IntPtr)HTTOPRIGHT;
                        else if(vPoint.Y >= ClientSize.Height - 5)
                            m.Result = (IntPtr)HTBOTTOMRIGHT;
                        else m.Result = (IntPtr)HTRIGHT;
                    else if(vPoint.Y <= 5)
                        m.Result = (IntPtr)HTTOP;
                    else if(vPoint.Y >= ClientSize.Height - 5)
                        m.Result = (IntPtr)HTBOTTOM;
                    break;
                case 0x0201://鼠标左键按下的消息 用于实现拖动窗口功能
                    m.Msg = 0x00A1;//更改消息为非客户区按下鼠标
                    m.LParam = IntPtr.Zero;//默认值
                    m.WParam = new IntPtr(2);//鼠标放在标题栏内
                    base.WndProc(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        //允许无边框窗口的最小化
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;  // Winuser.h中定义
                CreateParams cp = base.CreateParams;
                cp.Style = cp.Style | WS_MINIMIZEBOX;   // 允许最小化操作
                return cp;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        //加载配置文件以及初始化
        private void Form1_Load(object sender, EventArgs e)
        {
            Data.load_config();
            richTextBox1.ForeColor = ColorTranslator.FromHtml(Data.config["font_color"]);
            richTextBox1.Font = new Font(new FontFamily(Data.config["font_family"]), float.Parse(Data.config["font_size"]), FontStyle.Regular);
            Text_edit.SetLineSpace(richTextBox1, int.Parse(Data.config["line_space"]));
            iconToolStripButton2.Enabled = false;
        }

        [DllImport("user32.dll")]//拖动无窗体的控件
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;

        //panel充当标题栏的拖动功能
        private void Start_MouseDown(object sender, MouseEventArgs e)
        {
            //拖动窗体
            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }

        //自定义最小化窗口按钮
        private void iconButton2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        //自定义关闭窗口按钮
        private void iconButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //自定义最大化以及复原按钮
        private void iconButton3_Click(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case FormWindowState.Normal:
                    {
                        this.WindowState = FormWindowState.Maximized;
                        iconButton3.IconChar = FontAwesome.Sharp.IconChar.WindowRestore;
                        break;
                    }
                case FormWindowState.Maximized:
                    {
                        this.WindowState = FormWindowState.Normal;
                        iconButton3.IconChar = FontAwesome.Sharp.IconChar.WindowMaximize;
                        break;
                    }
                default:
                    break;
            }
        }

        //文件菜单被点击时设定哪些子菜单可用
        private void 文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (file_path != "")
            {
                关闭ToolStripMenuItem.Enabled = true;
                另存为ToolStripMenuItem.Enabled = true;
            }
            else
            {
                关闭ToolStripMenuItem.Enabled = false;
                另存为ToolStripMenuItem.Enabled = false;
            }
            if (text != "")
                保存ToolStripMenuItem.Enabled = true;
            else
                保存ToolStripMenuItem.Enabled = false;
        }

        //打开文件
        private void Open_file(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = false;
            open.Title = "请选择文本";
            open.Filter = "文本文件(*.txt)|*.txt";
            if(Data.config["text_folder"]!=""||!File.Exists(Data.config["text_folder"]))
                open.InitialDirectory = Data.config["text_folder"];
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                关闭ToolStripMenuItem_Click(this, e);
                text = "";
                file_path = open.FileName;
                text = File.ReadAllText(open.FileName);
                richTextBox1.Text = text;
                label3.Text = open.SafeFileName + "-" + this.Text;
                treeView_add(open);
            }
        }

        //添加进文件目录
        private void treeView_add(OpenFileDialog open)
        {
            TreeNodeCollection fliter_nodes = treeView1.Nodes;
            string path = open.FileName.Replace(open.SafeFileName, "");
            bool has_root = false;
            TreeNode root = null;
            foreach (TreeNode node in fliter_nodes)
            {
                if (path == node.Text)
                {
                    has_root = true;
                    root = node;
                    break;
                }
            }
            TreeNode new_node = new TreeNode();
            new_node.Tag = file_path;
            new_node.Text = open.SafeFileName;
            if (has_root)
            {
                root.Nodes.Add(new_node);
            }
            else
            {
                TreeNode root_node = new TreeNode();
                root_node.Text = path;
                treeView1.Nodes.Add(root_node);
                root_node.Nodes.Add(new_node);
            }
        }

        //关闭选中文件
        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file_path = "";
            richTextBox1.Clear();
            label1.Text = "";
            label3.Text = "太极动作辅助编码V1.0";
            if (Data.cut_sentence_result.potentials != null) Data.cut_sentence_result.potentials.Clear();
            if (Data.sen_res != null) Data.sen_res.Clear();
            if (Data.cut_word_result != null) Data.cut_word_result.Clear();
            if (Data.part_of_speech_analysis_results != null) Data.part_of_speech_analysis_results.Clear();
            if (Data.syntactic_analysis_results != null) Data.syntactic_analysis_results.Clear();
        }

        //保存文件
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (file_path != "") File.WriteAllText(file_path, richTextBox1.Text.Replace("\n", "\r\n"));
            else 另存为ToolStripMenuItem_Click(this, e);
        }

        //文件另存
        private void 另存为ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Title = "请选择保存文件路径";
            saveFile.Filter = "文本文件(*.txt)|*.txt|JSON文件(*.json)|*.json";
            if (Data.config["json_folder"] != "" || !File.Exists(Data.config["json_folder"]))
                saveFile.InitialDirectory = Data.config["json_folder"];
            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file_name = saveFile.FileName;
                string extension = Path.GetExtension(file_name);
                switch (extension)
                {
                    case ".txt":
                        File.WriteAllText(file_name, richTextBox1.Text.Replace("\n", "\r\n"));
                        break;
                    case ".json":
                        if (Data.sen_res.Count > 0)
                        {
                            List<List<List<string>>> four_tuples = Five_tuples.start_cal();
                            File.WriteAllText(file_name, JsonConvert.SerializeObject(four_tuples));
                        }
                        else
                        {
                            MessageBox.Show("请先进行解析");
                        }
                        break;
                    default:
                        MessageBox.Show("暂不支持此文件格式");
                        break;
                }
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //判断菜单的可用性
        private void Use_button()
        {
            if (richTextBox1.SelectedText == "")
            {
                复制ToolStripMenuItem.Enabled = false;
                删除ToolStripMenuItem.Enabled = false;
                复制ToolStripMenuItem1.Enabled = false;
                删除ToolStripMenuItem1.Enabled = false;
            }
            else
            {
                复制ToolStripMenuItem.Enabled = true;
                删除ToolStripMenuItem.Enabled = true;
                复制ToolStripMenuItem1.Enabled = true;
                删除ToolStripMenuItem1.Enabled = true;
            }
        }

        private void 编辑ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Use_button();
        }


        private void 撤销ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Undo();
            撤销ToolStripMenuItem.Enabled = false;
            撤销ToolStripMenuItem1.Enabled = false;
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Paste();
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectedText = "";
        }

        private void 查找ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form search = new 查找(richTextBox1);
            search.Show();
        }

        private void 替换ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form search = new 替换(richTextBox1);
            search.Show();
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }

        private void 分步解析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (text != "")
            {
                分步解析 form = new 分步解析(text);
                form.Show();
            }
        }

        private void 解析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (text != "")
            {
                if (Data.cut_sentence_result.potentials != null)
                {
                    Data.cut_sentence_result.potentials.Clear();
                    Data.sen_res.Clear();
                }

                string jsonstr = Web.Post(text, Data.config["upload"]);
                if (jsonstr != "error")
                {
                    Data.Once once = JsonConvert.DeserializeObject<Data.Once>(jsonstr);
                    Data.cut_sentence_result.potentials = once.potentials;
                    Data.sen_res = once.sen_res;
                }
            }
        }

        private void 批量解析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> files = new List<string>();
            foreach (TreeNode node in treeView1.Nodes)
            {
                foreach (TreeNode node_lev2 in node.Nodes)
                {
                    files.Add((string)node_lev2.Tag);
                }
            }
            批量解析 form = new 批量解析(files);
            form.Show();
        }

        private void 动作编辑ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            动作编辑 form = new 动作编辑();
            form.Show();
        }

        private void 动作制作ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Data.config["poser_path"] == "" || !File.Exists(Data.config["poser_path"]))
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Multiselect = false;
                open.Title = "请选择程序";
                open.Filter = "可执行程序(*.exe)|*.exe";
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Data.config["poser_path"] = open.FileName;
                    Data.save_config();
                    System.Diagnostics.Process.Start(Data.config["poser_path"]);
                }
            }
            else
            {
                System.Diagnostics.Process.Start(Data.config["poser_path"]);
            }
        }

        private void 动作浏览ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Data.config["preview"]);
        }

        private void 配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            配置 form = new 配置();
            form.change += Form_change;
            form.Show();
        }

        //实例化配置被修改后的触发的事件
        private void Form_change()
        {
            richTextBox1.Clear();
            richTextBox1.ForeColor = ColorTranslator.FromHtml(Data.config["font_color"]);
            richTextBox1.Font = new Font(new FontFamily(Data.config["font_family"]), float.Parse(Data.config["font_size"]), FontStyle.Regular);
            Text_edit.SetLineSpace(richTextBox1, int.Parse(Data.config["line_space"]));
            text = File.ReadAllText(file_path);
            richTextBox1.Text = text;
        }

        private void 使用说明ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            说明 form = new 说明();
            form.Show();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            关于 form = new 关于();
            form.Show();
        }

        //文件树窗口的鼠标点击，用于弹出对应菜单
        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            Point click_point = new Point(e.X, e.Y);//鼠标点坐标
            TreeNode current_node = treeView1.GetNodeAt(click_point);//选中的结点
            if (current_node != null)
            {
                treeView1.SelectedNode = current_node;
                label1.Text = (string)current_node.Tag;
                if (e.Button == MouseButtons.Right)
                {
                    if(current_node.Level==1) current_node.ContextMenuStrip = contextMenuStrip2;
                    if (current_node.Level == 0) current_node.ContextMenuStrip = contextMenuStrip3;
                }
                if (e.Button == MouseButtons.Left && current_node.Level == 1) toolStripMenuItem1_Click(sender, e);
            }
        }

        private void 重命名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.LabelEdit = true;
            treeView1.SelectedNode.BeginEdit();
        }

        //重命名后更改文件在文件系统中的名称
        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            string path = (string)treeView1.SelectedNode.Tag;
            string file_name = Path.GetFileName(path);
            treeView1.SelectedNode.Tag = path.Replace(file_name, e.Label);
            File.Move(path, (string)treeView1.SelectedNode.Tag);
            if (path == file_path) file_path = (string)treeView1.SelectedNode.Tag;
            treeView1.LabelEdit = false;
        }

        //左键点击子节点加载对应文件
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            关闭ToolStripMenuItem_Click(this, e);
            text = "";
            file_path = (string)treeView1.SelectedNode.Tag;
            text = File.ReadAllText(file_path);
            richTextBox1.Text = text;
            label3.Text = file_path.Split('\\').Last() + "-" + this.Text;
            label1.Text = (string)treeView1.SelectedNode.Tag;
        }

        //移除子节点文件
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (file_path == (string)treeView1.SelectedNode.Tag) 关闭ToolStripMenuItem_Click(this, e);
            if (treeView1.SelectedNode.Parent.Nodes.Count == 1)
            {
                treeView1.Nodes.Remove(treeView1.SelectedNode.Parent);
            }
            else
            {
                treeView1.Nodes.Remove(treeView1.SelectedNode);
            }
        }

        private void 删除ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            string path = (string)treeView1.SelectedNode.Tag;
            toolStripMenuItem2_Click(sender,e);
            File.Delete(path);
        }

        private void 新建文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file_name = "新建文本文件.txt";
            int num = 0;
            while (node_has_file(file_name))
            {
                num++;
                file_name = file_name.Replace(".txt", num.ToString() + ".txt");
            }
            File.Create(treeView1.SelectedNode.Text + file_name).Close();
            TreeNode node = new TreeNode();
            node.Text = file_name;
            node.Tag = treeView1.SelectedNode.Text + file_name;
            treeView1.SelectedNode.Nodes.Add(node);
        }

        private bool node_has_file(string file_name)
        {
            string[] files = Directory.GetFiles(treeView1.SelectedNode.Text, ".txt");
            foreach(string file in files)
            {
                if (file == file_name)
                {
                    return true;
                }
            }
            return false;
        }

        private void 关闭文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(TreeNode node in treeView1.SelectedNode.Nodes)
            {
                if ((string)node.Tag == file_path)
                {
                    关闭ToolStripMenuItem_Click(sender, e);
                    break;
                }
            }
            treeView1.Nodes.Remove(treeView1.SelectedNode);
        }

        //文本框右击呼出菜单后判断子菜单可用性
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            Use_button();
        }

        //文本框编辑后更改文本以及保存按钮可用性
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            撤销ToolStripMenuItem.Enabled = true;
            撤销ToolStripMenuItem1.Enabled = true;
            text = richTextBox1.Text.Replace("\n", "\r\n");
            if (text == "")
            {
                iconToolStripButton2.Enabled = false;
            }
            else
            {
                iconToolStripButton2.Enabled = true;
            }
        }

        //鼠标点击后光标在文本框的位置
        private void richTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            Point cursor_pos = Text_edit.Get_cursor_pos(richTextBox1);
            label1.Text = ("第" + cursor_pos.Y.ToString() + "行,第" + cursor_pos.X.ToString() + "列");
        }

        //方向键按动后光标在文本框的位置
        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            Point cursor_pos = Text_edit.Get_cursor_pos(richTextBox1);
            label1.Text = ("第" + cursor_pos.Y.ToString() + "行,第" + cursor_pos.X.ToString() + "列");
        }

        //自定义的分割布局
        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            mouse_position.X = e.X;
            mouse_position.Y = e.Y;
        }

        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point location_p3 = panel3.Location;
                Point location_rb = richTextBox1.Location;
                int change= e.X - mouse_position.X;
                treeView1.Width += change;
                location_p3.X += change;
                location_rb.X += change;
                richTextBox1.Location = location_rb;
                richTextBox1.Width = richTextBox1.Width - change;
                panel3.Location = location_p3;
                mouse_position.X = e.X;//更新鼠标位置
                mouse_position.Y = e.Y;
            }
        }
    }
}
