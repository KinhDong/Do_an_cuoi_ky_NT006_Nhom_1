using NT106.Forms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json; 
using NT106.Models;    
using System.Text;     
using System.Collections.Generic; 

namespace NT106
{
    public partial class Room : Form
    {
        private const string FirebaseBaseUrl = "https://nt106-cf479-default-rtdb.firebaseio.com/";

        private static readonly HttpClient client = new HttpClient();

        public Room()
        {
            InitializeComponent();
            this.FormClosing += AllForm.HandleFormClosing;
        }

        private void btn_CreateNewRoom_Click(object sender, EventArgs e)
        {
            CreateRoom f = new CreateRoom();
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;

            f.Show();
            this.Hide();
        }

        private void btn_Back_Click(object sender, EventArgs e)
        {
            Menu f = new Menu();
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;

            f.Show();
            this.Hide();
        }

        private async void btn_JoinRoom_Click(object sender, EventArgs e)
        {
            string roomCode = tb_RoomCode.Text.Trim();
            string userId = UserClass.Uid; // Lấy ID người dùng hiện tại
            string idToken = UserClass.IdToken; // Lấy token

            if (string.IsNullOrWhiteSpace(roomCode))
            {
                MessageBox.Show("Vui lòng nhập mã phòng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra token 
            if (string.IsNullOrWhiteSpace(idToken) || string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("Lỗi xác thực người dùng. Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Lấy dữ liệu phòng
                RoomClass roomData = await GetRoomDataAsync(roomCode);

                if (roomData == null)
                {
                    MessageBox.Show("Không tìm thấy phòng.", "Không tìm thấy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Kiểm tra xem phòng đã đầy chưa
                if (roomData.NumOfPlayers >= roomData.MaxPlayerCount)
                {
                    // Kiểm tra xem mình đã ở trong phòng chưa
                    if (roomData.Players == null || !roomData.Players.ContainsKey(userId))
                    {
                        MessageBox.Show("Phòng đã đầy.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    // Nếu đã ở trong phòng, cho phép vào lại
                }

                // Ghi dữ liệu người chơi mới lên Firebase 
                if (roomData.Players == null || !roomData.Players.ContainsKey(userId))
                {
                    // Tạo đối tượng người chơi mới
                    var newPlayerData = new
                    {
                        PlayerInGameName = UserClass.InGameName, 
                        JoinedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        PlayerMoney = UserClass.Money, 
                        isHost = false // Người tham gia không phải host
                    };

                    // Ghi dữ liệu người chơi
                    string playerUrl = $"{FirebaseBaseUrl}Rooms/{roomCode}/Players/{userId}.json?auth={idToken}";
                    var playerContent = new StringContent(JsonConvert.SerializeObject(newPlayerData), Encoding.UTF8, "application/json");
                    var playerResponse = await client.PutAsync(playerUrl, playerContent);

                    if (!playerResponse.IsSuccessStatusCode)
                    {
                        throw new Exception("Không thể ghi dữ liệu người chơi lên server.");
                    }

                    // Cập nhật số lượng người chơi
                    long newPlayerCount = roomData.NumOfPlayers + 1;
                    string numPlayersUrl = $"{FirebaseBaseUrl}Rooms/{roomCode}/NumOfPlayers.json?auth={idToken}";
                    // Ghi đè số lượng mới (dưới dạng JSON value)
                    var numContent = new StringContent(newPlayerCount.ToString(), Encoding.UTF8, "application/json");
                    var numResponse = await client.PutAsync(numPlayersUrl, numContent);

                    if (!numResponse.IsSuccessStatusCode)
                    {
                        throw new Exception("Không thể cập nhật số lượng người chơi.");
                    }

                    // Cập nhật đối tượng roomData 
                    roomData.NumOfPlayers = newPlayerCount;
                    if (roomData.Players == null) roomData.Players = new Dictionary<string, object>();
                    roomData.Players[userId] = newPlayerData;
                }

                // Chuyển sang form PlayAsPlayer
                PlayAsPlayer f = new PlayAsPlayer(roomData);
                f.StartPosition = FormStartPosition.Manual;
                f.Location = this.Location;

                f.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi tham gia phòng: " + ex.Message, "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<RoomClass> GetRoomDataAsync(string roomCode)
        {
            // Lấy IdToken từ UserClass
            string idToken = UserClass.IdToken;

            // Thêm token vào URL
            string requestUrl = $"{FirebaseBaseUrl}Rooms/{roomCode}.json?auth={idToken}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();

                    if (jsonContent == "null")
                    {
                        return null; // Phòng không tồn tại
                    }

                    RoomClass room = JsonConvert.DeserializeObject<RoomClass>(jsonContent);
                    return room;
                }
                else
                {
                    // In ra lỗi nếu request thất bại
                    Console.WriteLine($"[GetRoomData Error]: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return null; // Trả về null nếu có lỗi HTTP
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Lỗi kết nối mạng: " + ex.Message, ex);
            }
        }

        private void Room_Load(object sender, EventArgs e)
        {

        }
    }
}