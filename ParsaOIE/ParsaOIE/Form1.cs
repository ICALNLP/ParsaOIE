using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WcfService;

namespace ParsaOIE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnExtractOieRelations_Click(object sender, EventArgs e)
        {
            WcfService.IParsaWebService service = new ParsaWebService();
            string result = service.Parsa_Oie(richTextBox1.Text);
            MessageBox.Show(result);
        }
    }
}
