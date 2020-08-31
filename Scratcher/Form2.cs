using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scratcher
{
    public partial class Form2 : Form
    {
        Credentials C;
        public Form2(Credentials c)
        {
            InitializeComponent();
            C=c;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            C.domain = dt.Text;
            C.username = ut.Text;
            C.MakeSecureString(pmt.Text);
            dt.Text = "";
            ut.Text = "";
            pmt.Text = "";

            this.Close();
        }
    }
}
