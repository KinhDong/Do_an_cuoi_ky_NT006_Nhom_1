using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NT106.Scripts.Services;
using NT106.Scripts.Models;
using SystemHttpClient = System.Net.Http.HttpClient;

namespace NT106.Scripts.Models
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
        
        public static RoomClass CurrentRoom { get; set; }
        private static readonly SystemHttpClient http = new SystemHttpClient();
        // Tạo phòng mới
        public static async Task<RoomClass> CreateAsync(int maxPlayerCount, int betAmount)
        {
            try
            {
                if (UserClass.Money < betAmount * (maxPlayerCount - 1))
                {
                    GD.PrintErr("Không đủ tiền để tạo phòng");
                    return null;
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
                {
                    GD.PrintErr($"Không thể tạo phòng! Lỗi: {res.StatusCode}");
                    return null;
                }

                // Trả về room để thao tác trong Godot
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
                GD.PrintErr($"Lỗi tạo phòng: {ex.Message}");
                return null;
            }
        }

        // Tham gia phòng
        public static async Task<RoomClass> JoinAsync(string roomId)
        {
            try
            {
                var roomJson = await http.GetStringAsync($"{UserClass.DatabaseUrl}Rooms/{roomId}.json?auth={UserClass.IdToken}");
                if (roomJson == "null")
                {
                    GD.PrintErr("Phòng không tồn tại!");
                    return null;
                }
                    
                var roomData = JsonConvert.DeserializeObject<RoomClass>(roomJson);
                int current = roomData.CurrentPlayers;
                int max = roomData.MaxPlayers;

                // Kiểm tra phòng còn chỗ không
                if (current >= max)
                {
                    GD.PrintErr("Phòng đã đầy!");
                    return null;
                }                

                string playerUid = UserClass.Uid;
                string inGameName = UserClass.InGameName ?? "Guest";

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
                    GD.PrintErr("Không thể thêm người chơi");
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
            catch (Exception ex)
            {
                GD.PrintErr($"Lỗi tham gia phòng: {ex.Message}");
                return null;
            }
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
                GD.PrintErr($"[Lỗi rời phòng]: {ex.Message}");
                return false;
            }
        }

        // Xoá phòng
        public async Task<bool> DeleteAsync()
        {
            try
            {
                var response = await http.DeleteAsync($"{UserClass.DatabaseUrl}Rooms/{RoomId}.json?auth={UserClass.IdToken}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Lỗi xóa phòng: {ex.Message}");
                return false;
            }
        }

        // Lắng nghe thay đổi (đơn giản hóa cho Godot)
        public async Task ListenRoomChangesAsync(Action<RoomClass> onRoomUpdated, Action onRoomDeleted = null)
        {
            try
            {
                while (true)
                {
                    await Task.Delay(2000); // Kiểm tra mỗi 2 giây

                    var roomJson = await http.GetStringAsync($"{UserClass.DatabaseUrl}Rooms/{RoomId}.json?auth={UserClass.IdToken}");
                    
                    if (roomJson == "null")
                    {
                        onRoomDeleted?.Invoke();
                        break;
                    }

                    try
                    {
                        var updatedRoom = JsonConvert.DeserializeObject<RoomClass>(roomJson);
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
                        GD.PrintErr($"Lỗi Json: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Lỗi lắng nghe phòng: {ex.Message}");
            }
        }
    }
}