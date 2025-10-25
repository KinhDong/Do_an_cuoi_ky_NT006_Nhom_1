using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Linq;
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
        public static Image Avatar { get; set; }

        // === CẤU HÌNH FIREBASE ===
        private const string DatabaseUrl = "https://nt106-cf479-default-rtdb.firebaseio.com";
        private const string ApiKey = "AIzaSyD9_ECO_L-ex-4Iy_FkkstF8c6J2qaaW9Q";

        private static FirebaseClient firebaseClient = new FirebaseClient(DatabaseUrl);
        private static readonly string AuthDomain = "nt106-cf479.firebaseapp.com";

        private static readonly CloudinaryHelper cloudinary = new CloudinaryHelper();

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

                // Kiểm tra username đã tồn tại chưa
                var checkUsername = await firebaseClient
                    .Child("Usernames")
                    .Child(username)
                    .OnceSingleAsync<string>();

                if (!string.IsNullOrEmpty(checkUsername))
                    throw new Exception("Tên tài khoản đã tồn tại!");

                // Tạo người dùng Firebase
                var credential = await authClient.CreateUserWithEmailAndPasswordAsync(email, password);
                var user = credential.User;

                // Lưu thông tin username -> email
                await firebaseClient
                    .Child("Usernames")
                    .Child(username)
                    .PutAsync($"\"{email}\"");

                // Lưu thông tin người dùng vào Realtime Database
                await firebaseClient
                    .Child("Users")
                    .Child(user.Uid)
                    .PutAsync(new
                    {
                        Username = username,
                        Email = email,
                        InGameName = username,
                        Money = 1000,
                        isLoggedIn = false
                    });

                // Tải lên ảnh đại diện mặc định
                cloudinary.CopyImage(user.Uid);

                // Gửi email xác minh qua REST API
                string idToken = await user.GetIdTokenAsync();
                using (var client = new HttpClient())
                {
                    var content = new
                    {
                        requestType = "VERIFY_EMAIL",
                        idToken = idToken
                    };

                    var response = await client.PostAsync(
                        $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={ApiKey}",
                        new StringContent(System.Text.Json.JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
                    );

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("Không thể gửi email xác minh. Vui lòng thử lại sau.");
                    }
                }

                Console.WriteLine($"Đã gửi email xác minh đến: {email}");

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

                if (!user.Info.IsEmailVerified)
                    throw new Exception("Vui lòng xác minh email trước khi đăng nhập!");

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
                InGameName = userData.InGameName;
                Money = userData.Money;
                Avatar = await cloudinary.GetImageAsync($"avatar/{Uid}");

                return true;
            }
            catch (FirebaseAuthException)
            {
                throw new Exception("Sai mật khẩu hoặc tài khoản không tồn tại!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng nhập: {ex.Message}");
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

        //====== Lấy Email từ username ======
        public static async Task<string> GetEmailFromUsernameAsync(string username)
        {
            try
            {

                string email = await firebaseClient
                    .Child("Usernames")
                    .Child(username)
                    .OnceSingleAsync<string>();
                if (string.IsNullOrEmpty(email))
                    throw new Exception("Không tìm thấy tài khoản!");
                return email;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy email từ username: {ex.Message}");
                throw;
            }
        }

        // Quên và lấy lại mật khẩu
        public static async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                //Endpoint API của Firebase để gửi email đặt lại mật khẩu
                string url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={ApiKey}";
                var data = new
                {
                    requestType = "PASSWORD_RESET",
                    email = email
                };

                // Chuyển đổi dữ liệu thành JSON
                string json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gửi yêu cầu POST đến Firebase
                using (HttpClient client = new HttpClient())
                {
                    // Gửi yêu cầu
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    string result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Lỗi từ Firebase: {result}");
                        throw new Exception("Không thể gửi email đặt lại mật khẩu!");
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gửi email đặt lại mật khẩu: {ex.Message}");
                throw;
            }
        }

        // Thay đổi ảnh đại diện
        public static void ChangeAvatar(string filePath)
        {
            cloudinary.UploadImage(filePath, Uid);
            Avatar = Image.FromFile(filePath);
        }

        // Thay đổi tên trong game
        public static async void ChangeInGameName(string newName)
        {
            await UserClass.firebaseClient
                    .Child("Users")
                    .Child(UserClass.Uid)
                    .PatchAsync(new { InGameName = newName});

            UserClass.InGameName = newName;
        }
    }
}