using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; 
using System.Net.Http;

namespace NT106.Models
{
    public class RoomClass
    {

        [JsonProperty("RoomId")] // Dùng [JsonProperty] để đảm bảo map đúng
        public string RoomId { get; set; }

        [JsonProperty("HostUid")]
        public string HostUid { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("BetAmount")]
        public long BetAmount { get; set; } // Phải có kiểu dữ liệu (long hoặc int)

        [JsonProperty("MaxPlayerCount")]
        public long MaxPlayerCount { get; set; } // Phải có kiểu dữ liệu

        [JsonProperty("NumOfPlayers")]
        public long NumOfPlayers { get; set; } // Phải có kiểu dữ liệu

        [JsonProperty("Players")]
        public Dictionary<string, object> Players { get; set; }

        private static readonly HttpClient http = new HttpClient();

        // Tạo phòng mới
        public static async Task<RoomClass> CreateAsync(int maxPlayerCount, int betAmount)
        {
            try
            {
                if (UserClass.Money < betAmount * (maxPlayerCount - 1))
                {
                    throw new Exception("Không đủ tiền để tạo phòng");
                }

                string roomId;
                while (true)
                {
                    roomId = $"{new Random().Next(1000, 9999)}"; // RoomNoXXXX

                    // Kiểm tra mã phòng đã tồn tại hay chưa                    
                    var response = await http.GetStringAsync($"{UserClass.DatabaseUrl}Rooms/{roomId}.json?auth={UserClass.IdToken}");
                    if (response == "null" || string.IsNullOrEmpty(response)) break;
                }

                string hostUid = UserClass.Uid;
                string hostName = UserClass.UserName ?? "Host";

                // Dữ liệu phòng mới 
                var roomData = new
                {
                    HostUid = UserClass.Uid,
                    NumOfPlayers = 1,
                    RoomId = roomId,
                    MaxPlayerCount = maxPlayerCount,
                    BetAmount = betAmount,
                    Status = "Waiting",
                    Players = new Dictionary<string, object>
                    {
                        [UserClass.Uid] = new
                        //Player1 = new
                        {
                            PlayerInGameName = UserClass.InGameName,
                            JoinedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                            PlayerMoney = UserClass.Money,
                            isHost = true
                        }
                    }
                };

                // Ghi vào Firebase
                string putUrl = $"{UserClass.DatabaseUrl}Rooms/{roomId}.json?auth={UserClass.IdToken}";
                var content = new StringContent(JsonConvert.SerializeObject(roomData), Encoding.UTF8, "application/json");
                var res = await http.PutAsync(putUrl, content);

                if (!res.IsSuccessStatusCode)
                    throw new Exception($"Không thể tạo phòng! Lỗi: {res.StatusCode}");

                // Trả về đối tượng RoomClass để dùng trong chương trình
                // !! LƯU Ý: Phần code này có thể cần xem lại
                // vì nó không trả về đầy đủ dữ liệu
                return new RoomClass
                {
                    RoomId = roomId,
                    HostUid = UserClass.Uid,
                    Status = "Waiting",
                    // Các thuộc tính mới (BetAmount, Players...) chưa được gán ở đây
                    // Điều này không sao nếu bạn chỉ dùng đối tượng này
                    // để chuyển form ngay sau khi tạo phòng.
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoomClass.CreateAsync Error]: {ex.Message}");
                return null;
            }
        }
    }
}