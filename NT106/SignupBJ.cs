using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NT106;

namespace NT106
{
    public partial class SignupBJ : Form
    {
        public SignupBJ()
        {
            InitializeComponent();
        }

        private async void btn_CreateAccount_Click(object sender, EventArgs e)
        {
            string username = tb_AccountName.Text.Trim();
            string email = tb_Email.Text.Trim();
            string password = tb_Password.Text.Trim();
            string confirm = tb_ConfirmPassword.Text.Trim();

            try
            {
                bool result = await UserClass.RegisterAsync(username, email, password, confirm);

                if (result)
                {
                    MessageBox.Show("Đăng ký thành công! Hãy đăng nhập để tiếp tục.",
                                    "Thành công",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);

                    // Sau khi đăng ký, mở form đăng nhập (SigninBJ)
                    SigninBJ signinForm = new SigninBJ();

                    // Giữ nguyên vị trí form
                    signinForm.StartPosition = FormStartPosition.Manual;
                    signinForm.Location = this.Location;

                    signinForm.Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng ký: {ex.Message}",
                                "Lỗi",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
    }
}

