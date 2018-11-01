using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecureNote
{
    public partial class frmPassword1 : Form
    {
        string p1 = string.Empty; 

        public frmPassword1()
        {
            InitializeComponent();
            this.AcceptButton = btnOk;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            p1 = txtPassword1.Text;
            this.Close();
        }

        public string Password
        {
            get
            {
                return p1;
            }
        }
    }
}
