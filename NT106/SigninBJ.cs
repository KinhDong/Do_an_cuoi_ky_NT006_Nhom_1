using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Auth;
using System.Reactive.Linq;
using Firebase.Auth.Providers;

namespace NT106
{
    public partial class SigninBJ : Form
    {
        // === CẤU HÌNH FIREBASE ===
        private const string FirebaseUrl = "https://nt106-cf479-default-rtdb.firebaseio.com";
        private const string WebApiKey = "AIzaSyD9_ECO_L-ex-4Iy_FkkstF8c6J2qaaW9Q";

        private readonly FirebaseAuthClient authClient;
        private readonly FirebaseClient firebaseClient;

        public SigninBJ()
        {
            InitializeComponent();
            tb_Password.PasswordChar = '*';

            var config = new FirebaseAuthConfig
            {
                ApiKey = WebApiKey,
                AuthDomain = "nt106-cf479.firebaseapp.com",

                Providers = new FirebaseAuthProvider[]
                {
                    // Báo cho thư viện biết bạn sẽ dùng Email/Password
                    new EmailProvider()
                }
            };
            authClient = new FirebaseAuthClient(config);
            firebaseClient = new FirebaseClient(FirebaseUrl);
        }

        public class User
        {
            public string AccountName { get; set; }
            public string Email { get; set; }

            // Lưu Session ID hiện tại
            public string CurrentSessionId { get; set; }
        }

        private async void btnSignin_Click(object sender, EventArgs e)
        {
            string username = tb_AccountName.Text.Trim();
            string password = tb_Password.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập đủ tên tài khoản và mật khẩu.", "Thiếu thông tin",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btn_SignIn.Text = "Đang...";
            this.Enabled = false;

            try
            {
                // Hàm đăng nhập giờ sẽ trả về một Tuple chứa nhiều thông tin hơn
                var (loggedInUser, userCredential, userKey) = await KiemTraDangNhap(username, password);

                this.Enabled = true;
                btn_SignIn.Text = "Đăng nhập";

                if (loggedInUser != null && userCredential != null && !string.IsNullOrEmpty(userKey))
                {
                    // KIỂM TRA XEM CÓ AI ĐANG ĐĂNG NHẬP KHÔNG
                    if (!string.IsNullOrEmpty(loggedInUser.CurrentSessionId))
                    {
                        var result = MessageBox.Show(
                            "Tài khoản này đang được đăng nhập ở một nơi khác. Bạn có muốn tiếp tục đăng nhập và đăng xuất thiết bị kia không?",
                            "Cảnh báo đăng nhập",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.No)
                        {
                            tb_Password.Clear();
                            return;
                        }
                    }

                    // Đăng nhập THÀNH CÔNG
                    MessageBox.Show($"Chào mừng {loggedInUser.AccountName}!", "Đăng nhập thành công",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Tạo Session ID và cập nhật lên Firebase
                    string mySessionId = Guid.NewGuid().ToString();
                    await firebaseClient
                        .Child("Users")
                        .Child(userKey) // Dùng userKey để xác định đúng user cần cập nhật
                        .PatchAsync(new { CurrentSessionId = mySessionId });


                    // Mở form Menu và truyền các thông tin cần thiết
                    /*Menu menuForm = new Menu(firebaseClient, loggedInUser, userKey, mySessionId);
                    menuForm.FormClosed += (s, args) => this.Close();
                    menuForm.Show();
                    this.Hide();*/
                }
                else
                {
                    // Đăng nhập THẤT BẠI
                    MessageBox.Show("Sai tên tài khoản hoặc mật khẩu.", "Lỗi đăng nhập",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    tb_Password.Clear();
                    tb_AccountName.Focus();
                    tb_AccountName.SelectAll();
                }
            }
            catch (Exception ex)
            {
                this.Enabled = true;
                btn_SignIn.Text = "Đăng nhập";
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Hàm này giờ trả về một Tuple chứa User, UserCredential và Key của user trong DB
        private async Task<(User, UserCredential, string)> KiemTraDangNhap(string username, string password)
        {
            try
            {
                // BƯỚC 1: LẤY EMAIL VÀ KEY TỪ Accountname
                var users = await firebaseClient
                    .Child("Account")
                    .OrderBy("AccountName")
                    .EqualTo(username)
                    .OnceAsync<User>();

                var userNode = users.FirstOrDefault();

                if (userNode == null || string.IsNullOrWhiteSpace(userNode.Object.Email))
                {
                    return (null, null, null); // Không tìm thấy user
                }

                var user = userNode.Object;
                var userKey = userNode.Key; // Lấy được key của user

                // BƯỚC 2: ĐĂNG NHẬP BẰNG EMAIL/PASSWORD
                var userCredential = await authClient
                    .SignInWithEmailAndPasswordAsync(user.Email, password);

                // Nếu không văng lỗi, trả về bộ 3 giá trị
                return (user, userCredential, userKey);
            }
            catch (FirebaseAuthException authEx)
            {
                // Lỗi xác thực (sai mật khẩu, email không tồn tại trong Auth)
                MessageBox.Show($"Lỗi Authentication: {authEx.Reason}", "Lỗi Auth",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                return (null, null, null);
            }
        }


        private void SigninBJ_Load(object sender, EventArgs e)
        {
            tb_AccountName.Focus();
        }

        private void btnSignup_Click(object sender, EventArgs e)
        {
            SignupBJ signupForm = new SignupBJ();
            this.Hide();
            signupForm.ShowDialog();
            this.Show();
        }

        private void lbForgotPass_Click(object sender, EventArgs e)
        {
            ForgetPassword forgetPassForm = new ForgetPassword();
            this.Hide();
            forgetPassForm.ShowDialog();
            this.Show();
        }
    }
}