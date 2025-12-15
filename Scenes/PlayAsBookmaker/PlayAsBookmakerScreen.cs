using Godot;
using System;
using NT106.Scripts.Models;
using System.Collections.Generic;
using NT106.Scripts.Services;
using System.Linq;
using System.Threading.Tasks;
using Godot.Collections;

public partial class PlayAsBookmakerScreen : Node2D
{
	// Hiển thị mã phòng và mức cược
	[Export] private LineEdit DisplayRoomId;
	[Export] private LineEdit DisplayBetAmount;	

	// Hiển thị thông tin người chơi
	[Export] Array<DisplayPlayerInfo> DisplayPlayerInfos;
	
	[Export] private Button LeaveRoom; // Rời phòng

	FirebaseStreaming EventListener;

	// --------------- Ván chơi -----------------
	[Export] private Button StartGameButton;

	HashSet<(int, int)> DeckOfCards; // Bộ bài
	Random ran;
	List<string> TurnOrder; // Lượt
	[Export] AnimationPlayer anim; // animation
	Sprite2D[,] DisplayCards; // Hiển thị các lá bài

	private int currentTurn;

	public override void _Ready()
	{
		// Hiển thị thông tin chung
		DisplayRoomId.Text = RoomClass.CurrentRoom.RoomId;
		DisplayBetAmount.Text = RoomClass.CurrentRoom.BetAmount.ToString();

		// Hiển thị thông tin bản thân
		DisplayPlayerInfos[0].Display(RoomClass.CurrentRoom.Players[UserClass.Uid]);

		LeaveRoom.Pressed += OnLeaveRoomPressed;

		// Bắt đầu game
		StartGameButton.Pressed += OnStartGame;

		// Khởi tạo bộ bài
		DeckOfCards = new();
		for(int rank = 1; rank <= 13; rank++)        
			for(int suit = 1; suit <= 4; suit++)
				DeckOfCards.Add((rank, suit));

		ran = new();
		TurnOrder = new(); // Lấy lượt		

		// Hiển thị các lá bài
		DisplayCards = new Sprite2D[4, 5];
		for(int playerIndex = 0; playerIndex < 4; playerIndex++)				
			for(int cardIndex = 0; cardIndex < 5; cardIndex++)			
				DisplayCards[playerIndex, cardIndex] = 
				GetNode<Sprite2D>($"pn_Background/ttr_Table/CardsOfPlayer{playerIndex}/Card{cardIndex}");
						
		// Lắng nghe sự kiện từ Firebase
		EventListener = new(FirebaseApi.BaseUrl, $"Rooms/{RoomClass.CurrentRoom.RoomId}/Events", UserClass.IdToken);

		EventListener.OnConnected += () => GD.Print("Firebase connected");
		EventListener.OnDisconnected += () => GD.Print("Firebase disconnected");
		EventListener.OnError += (msg) => GD.Print("ERR: " + msg);

		EventListener.OnData += (json) =>
		{
			var evt = json.ToObject<MessageEvent>();

			if(evt != null && evt.data != null)
			{
				_ = Update(evt.data);
			}
		};		

		EventListener.Start();
	}

	private async void OnLeaveRoomPressed()
	{
		LeaveRoom.Disabled = true;
		GD.Print($"Đang xóa phòng: {RoomClass.CurrentRoom.RoomId}");

		try
		{
			var deleteSuccess = await RoomClass.CurrentRoom.DeleteAsync();

			if (deleteSuccess.Item1)
			{
				GD.Print("Xóa phòng thành công!");
				
				EventListener.Stop();
				RoomClass.CurrentRoom = null;				

				GetTree().ChangeSceneToFile(@"Scenes\CreateRoom\CreateRoom.tscn");				
			}

			else
			{
				GD.PrintErr(deleteSuccess.Item2);
				LeaveRoom.Disabled = false;
			}			
		}
		
		catch (Exception ex)
		{
			GD.PrintErr($"Lỗi khi xóa phòng: {ex.Message}");
			LeaveRoom.Disabled = false;
		}
	}

	private Task Update(RoomEvent evtData)
	{
		switch (evtData.type)
		{
			case "join":
				CallDeferred(nameof(UpdateJoin), evtData.user);
				break;

			case "leave":
				CallDeferred(nameof(UpdateLeave), evtData.user);
				break;

			case "hit":
				CallDeferred(nameof(UpdateHit), evtData.user);
				break;

			case "stand":
				CallDeferred(nameof(UpdateStand), evtData.user);
				break;

			default:
				break;
		}
		return Task.CompletedTask;
	}

	private async void UpdateJoin(string Pid)
	{
		try
		{
			var newPlayer = await FirebaseApi.Get<PlayerClass>($"Rooms/{RoomClass.CurrentRoom.RoomId}/Players/{Pid}");
			await newPlayer.LoadAvatarAsync();
			newPlayer.Hands = new();
			RoomClass.CurrentRoom.Players.Add(Pid, newPlayer);

			// Lấy chỗ
			int newSeat = 1;
			while(RoomClass.CurrentRoom.Seats[newSeat] != null) newSeat++;        

			RoomClass.CurrentRoom.Players[Pid].Seat = newSeat;
			RoomClass.CurrentRoom.Seats[newSeat] = Pid; // Xếp chỗ ngồi
			RoomClass.CurrentRoom.CurrentPlayers++;
			DisplayPlayerInfos[newSeat].Display(newPlayer);
			DisplayPlayerInfos[newSeat].Visible = true;

			GD.Print("Người chơi mới vào phòng.");
		}
			
		catch (Exception ex) {GD.PrintErr(ex.Message);}
	}

	private void UpdateLeave(string Pid)
	{
		// Lấy lại chỗ
		int CurrSeat = RoomClass.CurrentRoom.Players[Pid].Seat;
		RoomClass.CurrentRoom.Players.Remove(Pid);
		RoomClass.CurrentRoom.Seats[CurrSeat] = null;
		DisplayPlayerInfos[CurrSeat].Visible = false;
	}

	
	//-------------------------Xử lý bắt đầu ván chơi----------------------------------------
	private async void OnStartGame()
	{
		if(RoomClass.CurrentRoom.CurrentPlayers < 2)
		{
			OS.Alert("Chưa đủ người để bắt đầu ván.");
			return;
		}

		bool started = await StartNewRound();
		if(started) StartGameButton.Disabled = true;
		else return;

		await DealCardInit();

		// Bắt đầu giai đoạn rút hoặc dằn
		currentTurn = 0;
		HitOrStandPlayer(TurnOrder[currentTurn]);
	}

	private async Task<bool> StartNewRound()
	{
		try
		{
			var startEvt = new RoomEvent
			{
				type = "start_game",
				time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
			};

			//Ghi event lên firebase
			await FirebaseApi.Post($"Rooms/{RoomClass.CurrentRoom.RoomId}/Events", startEvt);

			RoomClass.CurrentRoom.Status = "PLAYING";
			//cập nhật lên firebase
			await FirebaseApi.Put($"Rooms/{RoomClass.CurrentRoom.RoomId}/Status", "PLAYING");

			for(int i = 1; i < 4; i++) // Không lấy cái
			{
				string pid = RoomClass.CurrentRoom.Seats[i];
				if(pid != null) TurnOrder.Add(pid);
			}

			return true;
		}

		catch (Exception ex)
		{
			GD.PrintErr(ex.Message);
			return false;
		}
	}

	private async Task<bool> DealCard(string pid)
	{
		try
		{
			var card = DeckOfCards.ElementAt(ran.Next(DeckOfCards.Count)); // Bốc 1 lá
			DeckOfCards.Remove(card);			

			var DealEvt = new RoomEvent
			{
				type = "deal_card",
				user = pid,
				payload = card,
				time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
			};

			int cardIndex = RoomClass.CurrentRoom.Players[pid].Hands.Count;
			RoomClass.CurrentRoom.Players[pid].Hands.Add(cardIndex, card); // Thêm trên dữ liệu

			await FirebaseApi.Post(
				$"Rooms/{RoomClass.CurrentRoom.RoomId}/Events", 
				DealEvt);

			await FirebaseApi.Put(
				$"Rooms/{RoomClass.CurrentRoom.RoomId}/Players/{pid}/Hands/{cardIndex}", 
				card);
			
			if(pid != UserClass.Uid)
			{
				int seat = RoomClass.CurrentRoom.Players[pid].Seat;
				anim.Play($"DealPlayer{seat}");
				await Task.Delay(300);
				DisplayCards[seat, 0].Visible = true;			
			}

			else
			{				
				anim.Play($"DealCard{cardIndex}");
				await Task.Delay(300);
				ShowCard(0, cardIndex, card);
			}

			anim.Queue("RESET");
						
			return true;
		}

		catch (Exception ex)
		{
			GD.PrintErr(ex.Message);
			return false;
		}
	}

	private async Task DealCardInit() // Chia bài cho các player ban đầu
	{
		for(int i = 0; i < 2; i++)
			foreach (var pid in TurnOrder)
			{
				await Task.Delay(500);
				await DealCard(pid);
			}		
	}

	private async void HitOrStandPlayer(string playerId)
	{
		try
		{
			var evt = new RoomEvent
			{
				type = "hit_or_stand",
				user = playerId,
				time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
			};

			await FirebaseApi.Post
			($"Rooms/{RoomClass.CurrentRoom.RoomId}/Events", evt);
		}

		catch (Exception ex) {GD.PrintErr(ex.Message);}      
	}

	private async void HitOrStandBookmaker()
	{
		var Score = RoomClass.CurrentRoom.Players[UserClass.Uid].CaclulateScore();

		while(Score.Item2 == 1 && Score.Item1 < 17) // Ngừng nếu trúng các trường hợp đặc biệt
		{
			await Task.Delay(500);
			await DealCard(UserClass.Uid);
			Score = RoomClass.CurrentRoom.Players[UserClass.Uid].CaclulateScore();
		}

		ProcessResult(); // Bắt đầu tính điểm
	}

	private async void UpdateHit(string playerId)
	{
		await DealCard(playerId);
	}

	private async void UpdateStand(string playerId)
	{
		currentTurn++;
		if(currentTurn < TurnOrder.Count)		
			HitOrStandPlayer(TurnOrder[currentTurn]); // Tiến đến player tiếp theo	

		else HitOrStandBookmaker();
	}	

	private async void ProcessResult() // Quá trình đánh giá kết quả
	{
		try
		{         
			var evt = new RoomEvent
			{
				type = "result",
				time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
			};
			await FirebaseApi.Post($"Rooms/{RoomClass.CurrentRoom.RoomId}/Events", evt);

			(int, int) myScore = RoomClass.CurrentRoom.Players[UserClass.Uid].CaclulateScore();

			long betAmout = RoomClass.CurrentRoom.BetAmount;
			long myMoney = UserClass.Money;

			foreach (string pid in TurnOrder)
			{
				await Task.Delay(500);
				var pScore = RoomClass.CurrentRoom.Players[pid].CaclulateScore();
				long pMoney = RoomClass.CurrentRoom.Players[pid].Money;

				ShowCards(pid);				

				string result = Result(pScore, myScore);
				if (result == "win")
				{
					pMoney += betAmout;
					myMoney -= betAmout;
				} 
				else if (result == "lose")
				{
					pMoney -= betAmout;
					myMoney += betAmout;
				}

				RoomClass.CurrentRoom.Players[pid].Money = pMoney;
				DisplayPlayerInfos[RoomClass.CurrentRoom.Players[pid].Seat].UpdateMoney(pMoney);

				evt = new RoomEvent {
					type = result,
					user = pid,
					time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
				};

				await FirebaseApi.Post($"Rooms/{RoomClass.CurrentRoom.RoomId}/Events", evt);

				// Thêm animation chiến thắng, trường hợp đặc biệt, hiệu ứng cộng tiền
			}

			// Cập nhật:
			UserClass.Money = myMoney;
			
			DisplayPlayerInfos[0].UpdateMoney(myMoney);
			await FirebaseApi.Put($"Users/{UserClass.Uid}/Money", myMoney);

			if (myMoney < (RoomClass.CurrentRoom.CurrentPlayers - 1) * 
			RoomClass.CurrentRoom.BetAmount)
				OnLeaveRoomPressed(); // Thoát và xóa phòng

			EndRound();
		}

		catch (Exception ex) {GD.PrintErr(ex.Message);}
	}

	private async void EndRound()
	{
		try
		{
			var evt = new RoomEvent
			{
				type = "end_round",
				time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
			};

			await FirebaseApi.Post($"Rooms/{RoomClass.CurrentRoom.RoomId}/Events", evt);

			await Task.Delay(5000);

			RoomClass.CurrentRoom.Status = "WAITING";
			await FirebaseApi.Put(
				$"Rooms/{RoomClass.CurrentRoom.RoomId}/Status", "WATING");

			foreach (var p in RoomClass.CurrentRoom.Players)
			{
				foreach (var card in RoomClass.CurrentRoom.Players[p.Key].Hands.Values)            
					DeckOfCards.Add(card); // Lấy lại bài
				UnShowCards(p.Key); // Ẩn bài
				await FirebaseApi.Delete(
					$"Rooms/{RoomClass.CurrentRoom.RoomId}/Players/{p.Key}/Hands");
				RoomClass.CurrentRoom.Players[p.Key].Hands.Clear();				
			}
			TurnOrder.Clear();

			StartGameButton.Disabled = false; // Cho phép bắt đầu ván
		}
		
		catch (Exception ex) {GD.PrintErr(ex.Message);}
	}

	private string Result((int, int) Score1, (int, int) Score2) // Phân định kết quả
	{
		if (Score1.Item2 > Score2.Item2) return "win"; // Player win
		if (Score1.Item2 < Score2.Item2) return "lose"; // Bookmaker win
		if (Score1.Item2 != 1 && Score1.Item2 != 2) return "draw"; // draw
		return Score1.Item1 > Score2.Item1 ? "win" : "lose";
	}

	private void ShowCard(int seat, int cardIndex, (int, int) card) // Hiển thị 1 lá bài
	{
		DisplayCards[seat, cardIndex].Frame = (card.Item1 - 1) + (card.Item2 - 1) * 13;
		DisplayCards[seat, cardIndex].Visible = true;
	}

	private void ShowCards(string pid) // Hiển thị các lá bài của 1 player
	{
		int seat = RoomClass.CurrentRoom.Players[pid].Seat;

		for(int i = 0; i < RoomClass.CurrentRoom.Players[pid].Hands.Count; i++)        
			ShowCard(seat, i, RoomClass.CurrentRoom.Players[pid].Hands[i]);        
	}

	private void UnShowCards(string pid)
	{
		int seat = RoomClass.CurrentRoom.Players[pid].Seat;

		for(int i = 0; i < RoomClass.CurrentRoom.Players[pid].Hands.Count; i++)        
			DisplayCards[seat, i].Visible = false;
		DisplayCards[seat, 0].Frame = 52; // Mặt sau lá bài
	}
}
