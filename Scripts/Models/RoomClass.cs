using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SystemHttpClient = System.Net.Http.HttpClient;
using NT106.Scripts.Services;

namespace NT106.Scripts.Models
{
    public class RoomClass
    {
        public string RoomId { get; set; }
        public string HostId {get; set;}
        public string Status { get; set; }
        public int BetAmount { get; set; }
        public int CurrentPlayers { get; set; }
        public Dictionary<string, PlayerClass> Players { get; set; }        
        
        public static RoomClass CurrentRoom { get; set; }


        // Tạo phòng mới
        public static async Task<(bool, string)> CreateAsync(int betAmount)
        {
            try
            {
                if (UserClass.Money < betAmount * 3)
                {
                    throw new Exception("Không đủ tiền để tạo phòng");
                }

                string roomId;
                while (true)
                {
                    roomId = $"{new Random().Next(1000, 9999)}"; // RoomNoXXXX

                    // Kiểm tra mã phòng đã tồn tại hay chưa
                    var response = await FirebaseApi.GetRaw($"Rooms/{roomId}.json?auth={UserClass.IdToken}");
                    if (response == "null" || string.IsNullOrEmpty(response)) break;
                }

                string hostUid = UserClass.Uid;

                // Tạo thông tin cho Bookmaker              
                var player = new PlayerClass
                {
                    Uid = hostUid,                    
                    InGameName = UserClass.InGameName,
                    Money = UserClass.Money,
                    JoinedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // Dữ liệu phòng mới 
                var roomData = new RoomClass
                {
                    RoomId = roomId,
                    HostId = UserClass.Uid,
                    CurrentPlayers = 1,
                    BetAmount = betAmount,
                    Status = "Waiting",
                    Players = new Dictionary<string, PlayerClass>
                    {
                        [UserClass.Uid] = player
                    }                    
                };

                // Ghi vào Firebase
                var res = await FirebaseApi.Put($"Rooms/{roomId}.json?auth={UserClass.IdToken}", roomData);

                if (!res)
                {
                    throw new Exception("Không thể lưu dữ liệu lên Database");
                }

                // Trả về room để thao tác trong Godot
                CurrentRoom = roomData;
                CurrentRoom.Players[UserClass.Uid].Avatar = UserClass.Avatar;
                CurrentRoom.Players[UserClass.Uid].Seat = 0;

                return (true, "OK");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Tham gia phòng
        public static async Task<(bool, string)> JoinAsync(string roomId)
        {
            try
            {
                var roomJson = await FirebaseApi.GetRaw($"Rooms/{roomId}.json?auth={UserClass.IdToken}");
                if (roomJson == "null")
                {
                    throw new Exception("Phòng không tồn tại!");
                }                    
                var roomData = JsonConvert.DeserializeObject<RoomClass>(roomJson);

                // Kiểm tra phòng còn chỗ không
                if (roomData.CurrentPlayers == 4)  throw new Exception("Phòng đã đầy!");             

                var player = new PlayerClass
                {
                    Uid = UserClass.Uid,
                    InGameName = UserClass.InGameName,
                    Money = UserClass.Money,
                    JoinedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var res = await FirebaseApi.Put
                ($"Rooms/{roomId}/Players/{UserClass.Uid}.json?auth={UserClass.IdToken}", player);
                
                if (!res) throw new Exception("Không thể thêm người chơi");  

                player.Avatar = UserClass.Avatar;
                player.Seat = 1;
                // Thêm vào roomData
                roomData.Players.Add(UserClass.Uid, player);

                // Cập nhật số người chơi
                roomData.CurrentPlayers++;
                res = await FirebaseApi.Put($"Rooms/{roomId}/CurrentPlayers.json?auth={UserClass.IdToken}", roomData.CurrentPlayers);

                if(!res) throw new Exception ("Không thể cập nhật số người chơi");

                CurrentRoom = roomData;
                return (true, "OK");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Rời phòng
        public async Task<(bool, string)> LeaveAsync()
        {
            try
            {                
                // Xoá player khỏi phòng
                var delPlayer = await FirebaseApi.Delete($"Rooms/{RoomId}/Players/{UserClass.Uid}.json?auth={UserClass.IdToken}");
                if (!delPlayer)
                    throw new Exception("Không thể xóa player khỏi phòng");

                // Giảm currentPlayers
                CurrentPlayers--;
                var res = await FirebaseApi.Put($"Rooms/{RoomId}/CurrentPlayers.json?auth={UserClass.IdToken}", CurrentPlayers);

                if(!res) throw new Exception("Không thể giảm số người trong phòng");

                CurrentRoom = null;
                return (true, "OK");                
            }

            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Xoá phòng
        public async Task<(bool, string)> DeleteAsync()
        {
            try
            {
                var res = await FirebaseApi.Delete($"Rooms/{RoomId}.json?auth={UserClass.IdToken}");

                if(!res) throw new Exception("Không thể xóa phòng");

                CurrentRoom = null;
                return (true, "OK");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }                
    }
}