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
        public string RoomId { get; set; }
        public string HostUid { get; set; }
        public List<PlayerClass> playerClasses { get; set; } = new List<PlayerClass>();
        public string Status { get; set; } = "Waiting";
        private const string DatabaseUrl = "https://nt106-cf479-default-rtdb.firebaseio.com/";
        private static readonly HttpClient http = new HttpClient();

        // Tạo phòng mới
        public static async Task<RoomClass> CreateAsync()
        {
            try
            {
                string roomId = $"Room{new Random().Next(1000, 9999)}"; // RoomNoXXXX
                string hostUid = UserClass.Uid;
                string hostName = UserClass.UserName ?? "Host";

                // Dữ liệu phòng mới 
                var roomData = new
                {
                    HostUid = hostUid,
                    NumOfPlayers = 1,
                    RoomId = roomId,
                    Status = "Waiting",
                    Players = new
                    {
                        Players1 = new
                        {
                            InGameName = hostName,
                            JoinedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                            Money = 0,
                            isHost = true
                        }
                    }
                };

                // Ghi vào Firebase
                var content = new StringContent(JsonConvert.SerializeObject(roomData), Encoding.UTF8, "application/json");
                var res = await http.PutAsync($"{DatabaseUrl}Rooms/{roomId}.json?auth={UserClass.IdToken}", content);

                if (!res.IsSuccessStatusCode)
                    throw new Exception($"Không thể tạo phòng! Lỗi: {res.StatusCode}");

                // Trả về đối tượng RoomClass để dùng trong chương trình
                return new RoomClass
                {
                    RoomId = roomId,
                    HostUid = hostUid,
                    Status = "Waiting",
                    playerClasses = new List<PlayerClass>
            {
                new PlayerClass
                {
                    Uid = hostUid,
                    IsHost = true
                }
            }
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

