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

            if (string.IsNullOrWhiteSpace(roomCode))
            {
                MessageBox.Show("Vui lòng nhập mã phòng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btn_JoinRoom.Enabled = false;
            btn_JoinRoom.Text = "Đang kiểm tra...";

            try
            {
                RoomClass roomData = await GetRoomDataAsync(roomCode);

                if (roomData != null)
                {
                    // Truyền toàn bộ đối tượng RoomClass cho PlayAsPlayer
                    PlayAsPlayer f = new PlayAsPlayer(roomData);
                    f.StartPosition = FormStartPosition.Manual;
                    f.Location = this.Location;

                    f.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy phòng. Vui lòng kiểm tra lại mã phòng.", "Không tìm thấy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi kết nối: " + ex.Message, "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn_JoinRoom.Enabled = true;
                btn_JoinRoom.Text = "Tham gia qua mã";
            }
        }

        private async Task<RoomClass> GetRoomDataAsync(string roomCode)
        {
            // Lấy IdToken từ UserClass, giả sử nó đã được lưu sau khi đăng nhập
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

                    // Chuyển đổi JSON thành đối tượng RoomClass
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
                // Ném lỗi ra ngoài để btn_JoinRoom_Click xử lý
                throw new Exception("Lỗi kết nối mạng: " + ex.Message, ex);
            }
        }

        private void Room_Load(object sender, EventArgs e)
        {
            
        }
    }
}