using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfiumViewer;

namespace desktop
{
    public partial class 说明 : Form
    {
        public 说明()
        {
            InitializeComponent();
        }

        private void 说明_Load(object sender, EventArgs e)
        {
            pdfViewer1.Document = PdfDocument.Load("data/使用说明.pdf");
            pdfViewer1.ZoomMode = PdfViewerZoomMode.FitWidth;
        }
    }
}
