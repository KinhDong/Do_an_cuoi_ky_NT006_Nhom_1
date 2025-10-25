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
    public partial class ForgetPassword : Form
    {
        public ForgetPassword()
        {
            InitializeComponent();
        }

        private async void btn_Confirm_Click(object sender, EventArgs e)
        {
            try
            {
                string username = tb_AccountName.Text.Trim();
                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Vui lòng nhập tên tài khoản");
                    return;
                }

                //Lấy email tương ứng từ username
                string email = await UserClass.GetEmailFromUsernameAsync(username);
                if (string.IsNullOrEmpty(email))
                {
                    MessageBox.Show("Không tìm thấy tài khoản");
                    return;
                }

                //Gửi email reset mật khẩu
                bool result = await UserClass.SendPasswordResetEmailAsync(email);
                if (result)
                    MessageBox.Show("Đã gửi email đặt lại mật khẩu", "Thành công");
            }

            catch (Exception ex)
            {
                MessageBox.Show("Lỗi" + ex.Message);
            }
        }
    }
}
