using Firebase.Database;
using Firebase.Database.Query;
using System.Reactive.Linq;
using Firebase.Auth;
using Firebase.Auth.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace NT106
{
    public static class UserClass
    {
        public static string Uid { get; private set; }
        public static string UserName { get; set; }
        public static string Email { get; set; }
        public static string InGameName { get; set; }
        public static long Money { get; set; }
        public static Byte[] Avatar { get; set; }

        // === CẤU HÌNH FIREBASE ===
        private const string DatabaseUrl = "https://nt106-cf479-default-rtdb.firebaseio.com";
        private const string ApiKey = "AIzaSyD9_ECO_L-ex-4Iy_FkkstF8c6J2qaaW9Q";

        private static FirebaseClient firebaseClient = new FirebaseClient(DatabaseUrl);
        private static readonly string AuthDomain = "nt106-cf479.firebaseapp.com";

        private static readonly FirebaseAuthConfig authConfig = new FirebaseAuthConfig
        {
            ApiKey = ApiKey,
            AuthDomain = AuthDomain,
            Providers = new FirebaseAuthProvider[]
            {
                new EmailProvider() // Cho phép đăng nhập bằng Email/Password
            }
        };
        private static readonly FirebaseAuthClient authClient = new FirebaseAuthClient(authConfig);

        // Đăng kí bằng Email + Username + Password and Confirm Password
        public static async Task<bool> RegisterAsync(string username, string email, string password, string confirm)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                    string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
                    throw new Exception("Vui lòng điền đầy đủ thông tin!");

                if (password != confirm)
                    throw new Exception("Mật khẩu xác nhận không khớp!");

                // Kiểm tra username đã tồn tại hay chưa
                var checkUsername = await firebaseClient
                    .Child("Usernames")
                    .Child(username)
                    .OnceSingleAsync<string>();

                if (!string.IsNullOrEmpty(checkUsername))
                    throw new Exception("Tên tài khoản đã tồn tại!");

                // Tạo người dùng trong Firebase Authentication
                var credential = await authClient.CreateUserWithEmailAndPasswordAsync(email, password);
                string uid = credential.User.Uid;

                // Lưu thông tin mapping username -> email
                await firebaseClient
                    .Child("Usernames")
                    .Child(username)
                    .PutAsync($"\"{email}\"");

                // Lưu thông tin người dùng trong "Users"
                var userData = new
                {
                    Username = username,
                    Email = email,
                    InGameName = username,
                    Money = 0,
                    isLoggedIn = false
                };

                await firebaseClient
                    .Child("Users")
                    .Child(uid)
                    .PutAsync(userData);

                return true;
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine($"Lỗi FirebaseAuth: {ex.Reason}");
                throw new Exception("Không thể đăng ký tài khoản! Email có thể đã được sử dụng.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng ký: {ex.Message}");
                throw;
            }
        }

        // ====== Đăng nhập bằng username + password ======
        public static async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                // Lấy email từ username mapping
                string email = await firebaseClient
                    .Child("Usernames")
                    .Child(username)
                    .OnceSingleAsync<string>();

                if (string.IsNullOrEmpty(email))
                    throw new Exception("Không tìm thấy tài khoản!");

                // Đăng nhập qua Firebase Authentication
                var credential = await authClient.SignInWithEmailAndPasswordAsync(email, password);
                var user = credential.User;
                string uid = user.Uid;

                // Lấy thông tin user từ Realtime Database
                var userData = await firebaseClient
                    .Child("Users")
                    .Child(uid)
                    .OnceSingleAsync<dynamic>();

                // Không cho đăng nhập khi đang đăng nhập ở nơi khác
                bool isLoggedIn = userData.isLoggedIn;
                if (isLoggedIn)
                    throw new Exception("Tài khoản này đang đăng nhập ở thiết bị khác!");

                // Đánh dấu đang đăng nhập
                await firebaseClient
                    .Child("Users")
                    .Child(uid)
                    .PatchAsync(new { isLoggedIn = true }); // Đánh dấu đang đăng nhập

                // Lưu thông tin vào UserClass
                Uid = uid;
                UserName = username;
                Email = email;
                InGameName = userData.InGameName ?? username;
                // Money = userData.money ?? username;

                return true;
            }
            catch (FirebaseAuthException)
            {
                throw new Exception("Sai mật khẩu hoặc tài khoản không tồn tại!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng nhập: {ex.Message}");
                return false;
            }
        }

        // ====== Đăng xuất ======
        public static async Task LogoutAsync()
        {
            if (string.IsNullOrEmpty(Uid)) return;

            try
            {
                // Cập nhật trạng thái trong database
                await firebaseClient
                    .Child("Users")
                    .Child(Uid)
                    .PatchAsync(new { isLoggedIn = false });

                // Xóa trạng thái đăng nhập Firebase
                authClient.SignOut();

                // Xóa thông tin local
                Uid = UserName = Email = InGameName = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng xuất: {ex.Message}");
            }
        }
    }
}
