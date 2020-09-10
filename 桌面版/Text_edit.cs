using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace desktop
{

    class Text_edit
    {
        public const int WM_USER = 0x0400;
        public const int EM_GETPARAFORMAT = WM_USER + 61;
        public const int EM_SETPARAFORMAT = WM_USER + 71;
        public const long MAX_TAB_STOPS = 32;
        public const uint PFM_LINESPACING = 0x00000100;
        [StructLayout(LayoutKind.Sequential)]
        private struct PARAFORMAT2
        {
            public int cbSize;
            public uint dwMask;
            public short wNumbering;
            public short wReserved;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public short wAlignment;
            public short cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;
            public int dySpaceBefore;
            public int dySpaceAfter;
            public int dyLineSpacing;
            public short sStyle;
            public byte bLineSpacingRule;
            public byte bOutlineLevel;
            public short wShadingWeight;
            public short wShadingStyle;
            public short wNumberingStart;
            public short wNumberingStyle;
            public short wNumberingTab;
            public short wBorderSpace;
            public short wBorderWidth;
            public short wBorders;
        }

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref PARAFORMAT2 lParam);

        /// <summary>
        /// 设置行距
        /// </summary>
        /// <param name="ctl">控件</param>
        /// <param name="dyLineSpacing">间距</param>
        public static void SetLineSpace(Control ctl, int dyLineSpacing)
        {
            //1像素=15缇。
            dyLineSpacing = dyLineSpacing * 15;
            //4:dylinespace成员以  缇。的形式指定从一行到下一行的间距。控件使用指定的精确间距，即使dylinespace指定的值小于单个间距。
            //3:dylinespace成员以  缇。的形式指定从一行到下一行的间隔。但是，如果dylinespace指定的值小于单间距，则控件将显示单间距文本。
            byte bLineSpacingRule = (byte)3;
            PARAFORMAT2 fmt = new PARAFORMAT2();
            fmt.cbSize = Marshal.SizeOf(fmt);
            fmt.bLineSpacingRule = bLineSpacingRule;
            fmt.dyLineSpacing = dyLineSpacing;
            fmt.dwMask = PFM_LINESPACING;
            try
            {
                SendMessage(new HandleRef(ctl, ctl.Handle), EM_SETPARAFORMAT, bLineSpacingRule, ref fmt);
            }
            catch
            { }
        }

        [System.Runtime.InteropServices.DllImport("User32.DLL")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int iParam);

        private const int EM_LINEFROMCHAR = 0xC9;
        private const int EM_LINEINDEX = 0xBB;

        //取得行列位置
        public static  Point Get_cursor_pos(RichTextBox textBox)
        {
            Point cursor_pos = new Point(0, 0);
            int x, y;
            y = SendMessage(textBox.Handle, EM_LINEFROMCHAR, textBox.SelectionStart, 0);
            x = textBox.SelectionStart - SendMessage(textBox.Handle, EM_LINEINDEX, y, 0);
            cursor_pos.Y = ++y;
            cursor_pos.X = ++x;
            return cursor_pos;
        }
    }
}
