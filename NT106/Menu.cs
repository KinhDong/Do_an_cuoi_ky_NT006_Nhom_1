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
            this.FormClosing += MainForm_FormClosing;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Tự động đăng xuất khi đóng app
                await UserClass.LogoutAsync();
            }

            // Tắt hoàn toàn chương trình, ngăn ngừa chạy ngầm
            Environment.Exit(0);
        }
    }
}
