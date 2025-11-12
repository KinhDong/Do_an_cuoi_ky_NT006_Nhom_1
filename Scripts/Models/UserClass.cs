using Godot;
using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NT106.Scripts.Services;

namespace NT106.Scripts.Models
{
    public static class UserClass
    {
        public static string Uid { get; private set; }
        public static string UserName { get; set; }
        public static string InGameName { get; set; }
        public static long Money { get; set; }
        public static ImageTexture Avatar { get; set; }
        public static string IdToken { get; private set; }

        private const string ApiKey = "AIzaSyD9_ECO_L-ex-4Iy_FkkstF8c6J2qaaW9Q";
        public const string DatabaseUrl = "https://nt106-cf479-default-rtdb.firebaseio.com/";


        private static readonly System.Net.Http.HttpClient http = new System.Net.Http.HttpClient();


        private static readonly CloudinaryServices cloudinary = new CloudinaryServices();


        // Đăng ký
        public static async Task<bool> RegisterAsync(string username, string email, string password, string confirm)
        {
            try
            {
                if (password != confirm) throw new Exception("Mật khẩu xác nhận không khớp!");

                // Kiểm tra username đã tồn tại chưa
                var check = await http.GetAsync($"{DatabaseUrl}Usernames/{username}.json");
                if (check.IsSuccessStatusCode && check.Content.ReadAsStringAsync().Result.Contains("@"))
                    throw new Exception("Tên tài khoản đã tồn tại!");

                // Tạo tài khoản Auth
                var payload = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                var res = await http.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={ApiKey}",
                    new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

                if (!res.IsSuccessStatusCode)
                    throw new Exception("Không thể đăng ký! Email có thể đã được sử dụng.");

                var json = JsonConvert.DeserializeObject<dynamic>(await res.Content.ReadAsStringAsync());
                string newUid = json.localId;
                string newIdToken = json.idToken;

                // Lưu username -> email (public)
                await http.PutAsync($"{DatabaseUrl}Usernames/{username}.json?auth={newIdToken}",
                    new StringContent(JsonConvert.SerializeObject(email)));

                // Gửi email xác minh
                var verifyPayload = new
                {
                    requestType = "VERIFY_EMAIL",
                    idToken = newIdToken
                };

                var verifyResponse = await http.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={ApiKey}",
                    new StringContent(JsonConvert.SerializeObject(verifyPayload), Encoding.UTF8, "application/json"));

                if (!verifyResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[Firebase Verify Email Error]: {await verifyResponse.Content.ReadAsStringAsync()}");
                    throw new Exception("Không thể gửi email xác minh.");
                }

                // THÔNG BÁO: "Đăng ký thành công! Vui lòng kiểm tra email để xác minh tài khoản."
                return true;
            }

            catch
            {
                return false;
            }
        }

        // Đăng nhập 
        public static async Task<(bool, string)> LoginAsync(string username, string password)
        {
            try
            {
                // Lấy email
                var emailRes = await http.GetStringAsync($"{DatabaseUrl}Usernames/{username}.json");
                if (string.IsNullOrWhiteSpace(emailRes) || !emailRes.Contains("@"))
                    throw new Exception("Không tìm thấy tài khoản!");

                string email = JsonConvert.DeserializeObject<string>(emailRes);

                // Đăng nhập
                var payload = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };
                var res = await http.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={ApiKey}",
                    new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

                if (!res.IsSuccessStatusCode)
                    throw new Exception("Sai tài khoản hoặc mật khẩu!");

                var json = JsonConvert.DeserializeObject<dynamic>(await res.Content.ReadAsStringAsync());
                IdToken = json.idToken;
                Uid = json.localId;

                // Kiểm tra xác minh email qua accounts:lookup
                var lookupPayload = new { idToken = (string)json.idToken };
                var lookupRes = await http.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={ApiKey}",
                    new StringContent(JsonConvert.SerializeObject(lookupPayload), Encoding.UTF8, "application/json"));

                if (!lookupRes.IsSuccessStatusCode)
                    throw new Exception("Không thể kiểm tra trạng thái xác minh email!");

                var lookupJson = JsonConvert.DeserializeObject<dynamic>(await lookupRes.Content.ReadAsStringAsync());
                bool verified = lookupJson.users[0].emailVerified ?? false;

                if (!verified)
                    throw new Exception("Vui lòng xác minh email trước khi đăng nhập!");

                // Kiểm tra và tạo user data nếu chưa tồn tại
                var userCheck = await http.GetAsync($"{DatabaseUrl}Users/{Uid}.json?auth={IdToken}");
                string userDataStr = await userCheck.Content.ReadAsStringAsync();

                if (userDataStr == "null") // User data chưa tồn tại
                {
                    // Tạo user data mới
                    var userInfo = new
                    {
                        Username = username,
                        InGameName = username,
                        Money = 1000,
                        isLoggedIn = true
                    };

                    var createUserRes = await http.PutAsync(
                        $"{DatabaseUrl}Users/{Uid}.json?auth={IdToken}",
                        new StringContent(JsonConvert.SerializeObject(userInfo), Encoding.UTF8, "application/json"));

                    if (!createUserRes.IsSuccessStatusCode)
                        throw new Exception("Không thể tạo dữ liệu người dùng!");

                    // Copy ảnh đại diện mặc định
                    cloudinary.CopyImage(Uid);

                    // Gán giá trị mặc định
                    UserName = username;
                    InGameName = username;
                    Money = 1000;
                    Avatar = await cloudinary.GetImageAsync($"avatar/{Uid}");
                }

                else
                {
                    // User data đã tồn tại
                    var userData = JsonConvert.DeserializeObject<dynamic>(userDataStr);

                    if (userData.isLoggedIn == true)
                        throw new Exception("Tài khoản đang được đăng nhập ở nơi khác!");

                    // Cập nhật trạng thái đăng nhập
                    await http.PatchAsync(
                        $"{DatabaseUrl}Users/{Uid}.json?auth={IdToken}",
                        new StringContent("{\"isLoggedIn\":true}", Encoding.UTF8, "application/json"));

                    UserName = username;
                    InGameName = userData.InGameName;
                    Money = userData.Money;
                    Avatar = await cloudinary.GetImageAsync($"avatar/{Uid}");
                }

                return (true, "OK");
            }

            catch(Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Đăng xuất 
        public static async Task LogoutAsync()
        {
            if (string.IsNullOrEmpty(Uid)) return;

            await http.PatchAsync($"{DatabaseUrl}Users/{Uid}.json?auth={IdToken}",
                new StringContent("{\"isLoggedIn\":false}", Encoding.UTF8, "application/json"));

            Uid = UserName = InGameName = null;
            IdToken = null;
        }

        // Lấy email từ username
        public static async Task<string> GetEmailFromUsernameAsync(string username)
        {
            string emailRes = await http.GetStringAsync($"{DatabaseUrl}Usernames/{username}.json");
            return JsonConvert.DeserializeObject<string>(emailRes);
        }

        // Gửi reset password
        public static async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email không được để trống!");

            var payload = new
            {
                requestType = "PASSWORD_RESET",
                email
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var res = await http.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={ApiKey}",
                content
            );

            string responseBody = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {                
                throw new Exception("Không thể gửi email khôi phục mật khẩu! Vui lòng kiểm tra lại email.");
            }

            return true;
        }


        // Đổi avatar
        public async static void ChangeAvatar(string filePath)
        {
            cloudinary.UploadImage(filePath, Uid);
            Avatar = await cloudinary.GetImageAsync(Uid);
        }

        // Đổi tên trong game
        public static async Task ChangeInGameName(string newName)
        {
            await http.PatchAsync(
                $"{DatabaseUrl}Users/{Uid}.json?auth={IdToken}",
                new StringContent("{\"InGameName\":\"" + newName + "\"}", Encoding.UTF8, "application/json")
            );
            InGameName = newName;
        }
    }
}
