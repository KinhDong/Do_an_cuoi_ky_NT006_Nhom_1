using CloudinaryDotNet;
using Firebase.Database;
using Firebase.Database.Query;
using System.Net;

namespace NT106
{
    public partial class Account : Form
    {
        public Account()
        {
            InitializeComponent();

            // Đăng xuất khi đóng form (Nhấn 'X')
            this.FormClosing += AllForm.HandleFormClosing;
        }

        private async void Account_Load(object sender, EventArgs e)
        {
            // Hiển thị thông tin user
            tb_AccountName.Text = UserClass.UserName;
            tb_IngameName.Text = UserClass.InGameName;
            pic_Avatar.Image = UserClass.Avatar;
        }

        private void btn_Back_Click(object sender, EventArgs e)
        {
            Menu f = new Menu();

            // Form mới xuất hiện đúng vị trí form cũ
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;
            f.Show();

            this.Hide();
        }

        private async void btn_ChangeIngameName_Click(object sender, EventArgs e)
        {
            if (tb_IngameName.ReadOnly) tb_IngameName.ReadOnly = false;
            else
            {
                UserClass.ChangeInGameName(tb_IngameName.Text);
                tb_IngameName.ReadOnly = true;
            }
        }

        private void btn_ChangeAvatar_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    UserClass.ChangeAvatar(ofd.FileName);
                    pic_Avatar.Image = UserClass.Avatar;
                }

                catch (Exception ex)
                {

                }
            }
        }

        private void btn_SignOut_Click(object sender, EventArgs e)
        {
            UserClass.LogoutAsync();

            SigninBJ f = new SigninBJ();

            // Form mới xuất hiện đúng vị trí form cũ
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;
            f.Show();

            this.Hide();
        }
    }
}
