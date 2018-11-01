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
    public partial class frmPassword2 : Form
    {
        string p1 = string.Empty; 
        string p2 = string.Empty;

        public frmPassword2()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            p1 = txtPassword1.Text;
            p2 = txtPassword2.Text;

            if (p1 == String.Empty) return;
            if (p2 == String.Empty) return;

            if (p1 != p2)
            {
                txtPassword1.Text = string.Empty;
                txtPassword2.Text = string.Empty;
                MessageBox.Show("Passwords Do Not Match!", "Password Error");
                return;
            }
            else
            {
                this.Close();
            }
        }

        public string Password
        {
            get
            {
                return p1;
            }
            set
            {
                txtPassword1.Text = value;
                txtPassword2.Text = value;
            }
        }
    }
}
