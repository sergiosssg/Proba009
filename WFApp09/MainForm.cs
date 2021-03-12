using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFApp09
{
    public partial class mainForm : Form
    {

        private ModelLoginning login;

        public mainForm()
        {
            login = new ModelLoginning();
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flowLayoutPnl01_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnEnter2Program_Click(object sender, EventArgs e)
        {
            string userName, password;
            bool permitted = false;

            userName = tbUserName.Text.ToString();
            password = markedtbPWD.Text.ToString();

            login = new ModelLoginning();
            login.UserLogin = userName;
            login.UserPassword = password;

            AdmissionFacility admissionFacility = new AdmissionFacility(true);

            permitted = admissionFacility.tryAccess(login);

            if (permitted)
            {
                tb070.Text = userName + " logged";
            } else
            {
                tb070.Text = userName + " not logged";
            }


            //
            //
            //
            //
        }

        private void flowLayoutPnl060_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
