using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
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
    }
}