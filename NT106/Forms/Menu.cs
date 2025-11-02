using NT106.Forms;
using NT106.Models;
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
            tb_Money.Text = UserClass.Money.ToString();
        }

        private void btn_Account_Click(object sender, EventArgs e)
        {
            // Mở form tài khoản
            Account f = new Account();
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;

            f.Show();
            this.Hide();
        }

        private void btn_PVP_Click(object sender, EventArgs e)
        {
            Room f = new Room();
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;

            f.Show();
            this.Hide();
        }
    }
}
