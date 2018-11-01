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
    public partial class frmFind : Form
    {
        string f = string.Empty;

        public frmFind()
        {
            InitializeComponent();
            this.AcceptButton = btnFind;
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            f = txtFind.Text;
            if (f != string.Empty) this.Close();
        }

        public string Find
        {
            get
            {
                return f;
            }
        }
    }
}
