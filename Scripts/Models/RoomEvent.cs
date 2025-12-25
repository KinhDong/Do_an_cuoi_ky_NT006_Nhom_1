using System;
using System.Threading.Tasks;
using Godot;
using NT106.Scripts.Services;

namespace NT106.Scripts.Models
{
    public class RoomEvent
	{
		public string type { get; set; }   // "join", "leave", ...
		public string user { get; set; }   // userId
		public string time { get; set; }
		public dynamic payload { get; set; } // Dữ liệu thêm vào (Nếu có)
	
        public static async Task PostRoomEventAsync (string _roomId, string _type, string _userId = null, dynamic _payload = null)
        {
            try
            {
                var roomEvent = new RoomEvent
                {
                    type = _type,
                    user = _userId,
                    time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    payload = _payload
                };

                await FirebaseApi.Post($"Rooms/{_roomId}/Events", roomEvent);
            }

            catch (Exception ex)
            {
                GD.PrintErr($"Error posting room event: {ex.Message}");
            }      
        }
    }

    
}