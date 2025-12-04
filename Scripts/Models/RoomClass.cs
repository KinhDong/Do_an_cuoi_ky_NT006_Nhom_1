using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SystemHttpClient = System.Net.Http.HttpClient;
using NT106.Scripts.Services;
using System.Threading;
using System.Linq;

namespace NT106.Scripts.Models
{
	// Class chứa hằng số trạng thái phòng
	public static class RoomStatus
	{
		public const string WAITING = "Waiting"; // Đang chờ
		public const string PLAYING = "Playing"; // Đang chơi
	}

	// Enum định nghĩa thứ hạng bài (Để so sánh sức mạnh)
	public enum HandRank
	{
		Bust = 0,       // Quắc (> 21 điểm)
		Normal = 1,     // Bài thường (<= 21 điểm)
		XiDach = 2,     // Xì Dách (1 Át + 1 Tây/10)
		XiBang = 3      // Xì Bàng (2 Át)
	}

	// Tạo Room Event để ghi log sự kiện
	public class RoomEvent
	{
		public string type { get; set; } // "join", "leave", "deal", "start_game", "end_game", "change_turn", "request_action"
		public string user { get; set; } // userId
		public string action { get; set; } // "hit" hoặc "stand" (dùng cho request_action)
		public long time { get; set; }
		public CardClass card { get; set; }

		// --- Dữ liệu kết quả ---
		public Dictionary<string, long> results { get; set; } // Map: UserID -> Số tiền thắng/thua
		public int hostScore { get; set; } // Điểm của nhà cái
	}

	// Class wrap để hứng JSON từ Firebase Streaming
	public class MessageEvent
	{
		public string path { get; set; }
		public RoomEvent data { get; set; }
	}

	public class RoomClass
	{
		public string RoomId { get; set; }
		public string HostId { get; set; }
		public string Status { get; set; }
		public int BetAmount { get; set; }
		public int CurrentPlayers { get; set; }
		public Dictionary<string, PlayerClass> Players { get; set; }
		public string CurrentTurn { get; set; } // Lưu UID người đang đến lượt
		
		public static RoomClass CurrentRoom { get; set; }

		// ------------------------------------------------------------------------
		// LOGIC TÍNH ĐIỂM VÀ XẾP HẠNG BÀI (FULL LUẬT)
		// ------------------------------------------------------------------------

		public int CalculateScore(List<CardClass> cards)
		{
			if (cards == null || cards.Count == 0) return 0;

			int score = 0;
			int aceCount = 0;

			foreach (var card in cards)
			{
				if (card.Rank == "J" || card.Rank == "Q" || card.Rank == "K" || card.Rank == "10")
				{
					score += 10;
				}
				else if (card.Rank == "A")
				{
					aceCount++;
					score += 1; 
				}
				else
				{
					score += card.Value;
				}
			}

			// Logic đôn Át: Cộng thêm 10 cho mỗi quân Át nếu tổng không vượt quá 21
			for (int i = 0; i < aceCount; i++)
			{
				if (score + 10 <= 21)
				{
					score += 10;
				}
			}

			return score;
		}

		public (HandRank, int) GetHandStrength(List<CardClass> cards)
		{
			int score = CalculateScore(cards);

			// Kiểm tra bài đặc biệt chỉ áp dụng khi có đúng 2 lá
			if (cards.Count == 2)
			{
				bool hasAce = cards.Any(c => c.Rank == "A");
				bool hasTenCard = cards.Any(c => c.Rank == "10" || c.Rank == "J" || c.Rank == "Q" || c.Rank == "K");
				int aceCount = cards.Count(c => c.Rank == "A");

				if (aceCount == 2) return (HandRank.XiBang, score);
				if (hasAce && hasTenCard) return (HandRank.XiDach, score);
			}

			if (score > 21) return (HandRank.Bust, score);
			
			return (HandRank.Normal, score);
		}

		// ------------------------------------------------------------------------
		// CÁC HÀM QUẢN LÝ PHÒNG (Create, Join, Leave, Delete) - GIỮ NGUYÊN
		// ------------------------------------------------------------------------

		public static async Task<(bool, string)> CreateAsync(int betAmount)
		{
			try
			{
				if (UserClass.Money < betAmount * 3) throw new Exception("Không đủ tiền để tạo phòng");

				string roomId;
				while (true)
				{
					roomId = $"{new Random().Next(1000, 9999)}"; 
					var response = await FirebaseApi.GetRaw($"Rooms/{roomId}.json?auth={UserClass.IdToken}");
					if (response == "null" || string.IsNullOrEmpty(response)) break;
				}

				var player = new PlayerClass
				{
					Uid = UserClass.Uid,                    
					InGameName = UserClass.InGameName,
					Money = UserClass.Money,
					JoinedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
				};

				var roomData = new RoomClass
				{
					RoomId = roomId,
					HostId = UserClass.Uid,
					CurrentPlayers = 1,
					BetAmount = betAmount,
					Status = RoomStatus.WAITING,
					Players = new Dictionary<string, PlayerClass> { [UserClass.Uid] = player }                    
				};

				var res = await FirebaseApi.Put($"Rooms/{roomId}.json?auth={UserClass.IdToken}", roomData);
				if (!res) throw new Exception("Không thể lưu dữ liệu lên Database");

				CurrentRoom = roomData;
				CurrentRoom.Players[UserClass.Uid].Seat = 0;

				return (true, "OK");
			}
			catch (Exception ex) { return (false, ex.Message); }
		}

		public static async Task<(bool, string)> JoinAsync(string roomId)
		{
			try
			{
				var roomJson = await FirebaseApi.GetRaw($"Rooms/{roomId}.json?auth={UserClass.IdToken}");
				if (roomJson == "null") throw new Exception("Phòng không tồn tại!");
									
				var roomData = JsonConvert.DeserializeObject<RoomClass>(roomJson);

				var player = new PlayerClass
				{
					Uid = UserClass.Uid,
					InGameName = UserClass.InGameName,
					Money = UserClass.Money,
					JoinedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
				};

				if (roomData.Status != RoomStatus.WAITING) throw new Exception("Phòng đã đang bắt đầu trò chơi!");
				else
				{
					if (roomData.CurrentPlayers >= 4) throw new Exception("Phòng đã đầy!");
					player.Seat = 1; 
				}   

				var res = await FirebaseApi.Put($"Rooms/{roomId}/Players/{UserClass.Uid}.json?auth={UserClass.IdToken}", player);
				if (!res) throw new Exception("Không thể thêm người chơi");  

				roomData.Players.Add(UserClass.Uid, player);
				roomData.CurrentPlayers++;
				await FirebaseApi.Put($"Rooms/{roomId}/CurrentPlayers.json?auth={UserClass.IdToken}", roomData.CurrentPlayers);

				CurrentRoom = roomData;

				var evt = new RoomEvent{type = "join", user = UserClass.Uid, time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()};
				await FirebaseApi.Post($"Rooms/{roomId}/Events.json?auth={UserClass.IdToken}", evt);

				return (true, "OK");
			}
			catch (Exception ex) { return (false, ex.Message); }
		}

		public async Task<(bool, string)> LeaveAsync()
		{
			try
			{                                
				var delPlayer = await FirebaseApi.Delete($"Rooms/{RoomId}/Players/{UserClass.Uid}.json?auth={UserClass.IdToken}");
				if (!delPlayer) throw new Exception("Không thể xóa player khỏi phòng");

				CurrentPlayers--;
				await FirebaseApi.Put($"Rooms/{RoomId}/CurrentPlayers.json?auth={UserClass.IdToken}", CurrentPlayers);

				CurrentRoom = null;

				var evt = new RoomEvent{type = "leave", user = UserClass.Uid, time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()};
				await FirebaseApi.Post($"Rooms/{RoomId}/Events.json?auth={UserClass.IdToken}", evt);

				return (true, "OK");                                
			}
			catch (Exception ex) { return (false, ex.Message); }
		}

		public async Task<(bool, string)> DeleteAsync()
		{
			try
			{
				var evt = new RoomEvent{type = "deleteRoom", user = UserClass.Uid, time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()};
				await FirebaseApi.Post($"Rooms/{RoomId}/Events.json?auth={UserClass.IdToken}", evt);

				await Task.Delay(2000); 

				var res = await FirebaseApi.Delete($"Rooms/{RoomId}.json?auth={UserClass.IdToken}");
				if(!res) throw new Exception("Không thể xóa phòng");

				CurrentRoom = null;     
				return (true, "OK");
			}
			catch (Exception ex) { return (false, ex.Message); }
		}                   
	}
}
