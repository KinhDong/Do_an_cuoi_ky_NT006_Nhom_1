using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NT106
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();

            // Đăng xuất khi đóng form (Nhấn 'X')
            this.FormClosing += AllForm.HandleFormClosing;
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            

        }

        private void btn_Account_Click(object sender, EventArgs e)
        {
            // Mở form tài khoản
            Account signinForm = new Account();
            signinForm.StartPosition = FormStartPosition.Manual;
            signinForm.Location = this.Location;

            signinForm.Show();
            this.Hide();
        }

        
    }
}
