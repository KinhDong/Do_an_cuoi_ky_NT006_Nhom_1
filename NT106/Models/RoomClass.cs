using Newtonsoft.Json; 
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NT106.Models
{
    public class RoomClass
    {
        public string RoomId { get; set; }
        public string HostUid { get; set; }
        public string Status { get; set; }
        public int BetAmount { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public Dictionary<string, PlayerClass> Players { get; set; }

        private static readonly HttpClient http = new HttpClient();

        private CancellationTokenSource cts;

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

                // Tạo 1 Player mới
                var player = new PlayerClass
                {
                    Uid = hostUid,
                    InGameName = UserClass.InGameName,
                    IsHost = true,
                    Money = UserClass.Money,
                    JoinedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // Dữ liệu phòng mới 
                var roomData = new
                {
                    HostUid = UserClass.Uid,
                    RoomId = roomId,
                    MaxPlayers = maxPlayerCount,
                    CurrentPlayers = 1,
                    BetAmount = betAmount,
                    Status = "Waiting",
                    Players = new Dictionary<string, object>
                    {
                        [UserClass.Uid] = player
                    }
                };

                // Ghi vào Firebase
                string putUrl = $"{UserClass.DatabaseUrl}Rooms/{roomId}.json?auth={UserClass.IdToken}";
                var content = new StringContent(JsonConvert.SerializeObject(roomData), Encoding.UTF8, "application/json");
                var res = await http.PutAsync(putUrl, content);

                if (!res.IsSuccessStatusCode)
                    throw new Exception($"Không thể tạo phòng! Lỗi: {res.StatusCode}");

                // Thêm ảnh đại diện
                player.Avatar = UserClass.Avatar;

                // Trả về room để thao tác trên winform
                return new RoomClass
                {
                    HostUid = UserClass.Uid,
                    RoomId = roomId,
                    MaxPlayers = maxPlayerCount,
                    CurrentPlayers = 1,
                    BetAmount = betAmount,
                    Status = "Waiting",
                    Players = new Dictionary<string, PlayerClass>
                    {
                        {hostUid, player}
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo phòng: {ex.Message}");
                return null;
            }
        }

        //Tham gia phòng
        public static async Task<RoomClass> JoinAsync(string roomId)
        {
            var roomJson = await http.GetStringAsync($"{UserClass.DatabaseUrl}Rooms/{roomId}.json?auth={UserClass.IdToken}");
            if (roomJson == "null")
            {
                MessageBox.Show("Phòng không tồn tại!");
                return null;
            }
                
            var roomData = JsonConvert.DeserializeObject<RoomClass>(roomJson);
            int current = roomData.CurrentPlayers;
            int max = roomData.MaxPlayers;

            // Kiểm tra phòng còn chỗ không
            if (current >= max)
            {
                MessageBox.Show("Phòng đã đầy!");
                return null;
            }                

            string playerUid = UserClass.Uid;
            string inGameName = UserClass.UserName ?? "Guest";

            var player = new PlayerClass
            {
                Uid = playerUid,
                InGameName = inGameName,
                IsHost = false,
                Money = UserClass.Money,
                JoinedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var json = JsonConvert.SerializeObject(player);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await http.PutAsync($"{UserClass.DatabaseUrl}Rooms/{roomId}/Players/{playerUid}.json?auth={UserClass.IdToken}", content);
            if (!res.IsSuccessStatusCode)
            {
                MessageBox.Show("Không thể thêm người chơi");
                return null;
            }                

            // Cập nhật số người chơi
            current++;
            var updateCount = new StringContent(current.ToString(), Encoding.UTF8, "application/json");
            await http.PutAsync($"{UserClass.DatabaseUrl}Rooms/{roomId}/CurrentPlayers.json?auth={UserClass.IdToken}", updateCount);

            // Tạo đối tượng RoomClass cho client
            roomData.RoomId = roomId;
            roomData.Players = roomData.Players ?? new Dictionary<string, PlayerClass>();
            roomData.Players.Add(player.Uid, player);

            return roomData;
        }

        // Rời phòng
        public async Task<bool> LeaveAsync()
        {
            try
            {
                string uid = UserClass.Uid;
                string url = $"{UserClass.DatabaseUrl}Rooms/{RoomId}.json?auth={UserClass.IdToken}";
                
                // Xoá player khỏi phòng
                var delPlayer = await http.DeleteAsync($"{UserClass.DatabaseUrl}Rooms/{RoomId}/Players/{uid}.json?auth={UserClass.IdToken}");
                if (!delPlayer.IsSuccessStatusCode)
                    return false;

                // Giảm currentPlayers
                var roomJson = await http.GetStringAsync(url);
                if (roomJson != "null")
                {
                    dynamic data = JsonConvert.DeserializeObject(roomJson);
                    int current = data.CurrentPlayers;
                    current--;

                    var updateCount = new StringContent(current.ToString(), Encoding.UTF8, "application/json");
                    await http.PutAsync($"{UserClass.DatabaseUrl}Rooms/{RoomId}/CurrentPlayers.json?auth={UserClass.IdToken}", updateCount);
                }
                return true;                
            }

            catch (Exception ex)
            {
                MessageBox.Show($"[Lỗi rời phòng]: {ex.Message}");
                return false;
            }
        }

        // Xoá phòng
        public async Task DeleteAsync()
        {
            await http.DeleteAsync($"{UserClass.DatabaseUrl}Rooms/{RoomId}.json?auth={UserClass.IdToken}");
        }

        // Lắng nghe thay đổi
        public async Task ListenRoomChangesAsync(Action<RoomClass> onRoomUpdated, Action onRoomDeleted = null)
        {
            try
            {
                cts = new CancellationTokenSource();

                string url = $"{UserClass.DatabaseUrl}Rooms/{RoomId}.json?auth={UserClass.IdToken}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

                var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);
                var stream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
                using var reader = new StreamReader(stream);

                string eventType = null;

                while (!reader.EndOfStream && !cts.Token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.StartsWith("event: "))
                    {
                        eventType = line.Substring(7);
                    }
                    else if (line.StartsWith("data: "))
                    {
                        var json = line.Substring(6).Trim();

                        // Nếu Firebase gửi data: null => phòng bị xóa
                        if (json == "null")
                        {
                            onRoomDeleted?.Invoke(); // gọi callback cho UI
                            break; // thoát khỏi vòng lặp
                        }

                        try
                        {
                            var updatedRoom = JsonConvert.DeserializeObject<RoomClass>(json);
                            if (updatedRoom != null)
                            {
                                RoomId = updatedRoom.RoomId ?? RoomId;
                                HostUid = updatedRoom.HostUid ?? HostUid;
                                Status = updatedRoom.Status ?? Status;
                                Players = updatedRoom.Players ?? Players;

                                onRoomUpdated?.Invoke(updatedRoom);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi Json: {ex.Message}");
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show("Lắng nghe đã bị dừng (hoặc hủy)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }


        // Ngừng lắng nghe
        public void StopListening()
        {
            cts?.Cancel();
        }
    }
}