using NT106.Forms;
using NT106.Models;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace NT106
{
    public partial class SigninBJ : Form
    {
        public SigninBJ()
        {
            InitializeComponent();

            this.FormClosing += AllForm.HandleFormClosing;
        }

        private async void btn_SignIn_Click(object sender, EventArgs e)
        {
            string username = tb_AccountName.Text.Trim();
            string password = tb_Password.Text;

            bool result = await UserClass.LoginAsync(username, password);

            if (result)
            {
                Menu f = new Menu();

                // Form mới xuất hiện đúng vị trí form cũ
                f.StartPosition = FormStartPosition.Manual;
                f.Location = this.Location;
                f.Show();

                this.Hide();
            }

            else
            {
                MessageBox.Show("Đăng nhập thất bại");
            }
        }

        private void btn_CreateAccount_Click(object sender, EventArgs e)
        {
            SignupBJ signupForm = new SignupBJ();

            // Giữ nguyên vị trí form hiện tại
            signupForm.StartPosition = FormStartPosition.Manual;
            signupForm.Location = this.Location;

            signupForm.FormClosed += (s, args) => this.Show();

            signupForm.Show();
            this.Hide();
        }

        private void btn_ForgotPassword_Click(object sender, EventArgs e)
        {
            ForgetPassword f = new ForgetPassword();

            // Form mới xuất hiện đúng vị trí form cũ
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;
            f.Show();

            this.Hide();
        }
    }
}