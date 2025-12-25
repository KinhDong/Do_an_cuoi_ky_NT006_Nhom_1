using Godot;
using System;
using NT106.Scripts.Models;
using System.Collections.Generic;
using NT106.Scripts.Services;
using System.Linq;
using System.Threading.Tasks;
using Godot.Collections;
using System.Net.Sockets;

public partial class PlayAsBookmakerScreen : Node2D
{
	RoomClass room {get; set;}
	// Hiển thị mã phòng và mức cược
	[Export] private LineEdit DisplayRoomId;
	[Export] private LineEdit DisplayBetAmount;	

	// Hiển thị thông tin người chơi
	[Export] Array<DisplayPlayerInfo> DisplayPlayerInfos;
	
	[Export] private Button LeaveRoom; // Rời phòng

	FirebaseStreaming EventListener;

	HeartbeatService heartbeatService;

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
		room = RoomClass.CurrentRoom;
		
		// Hiển thị thông tin chung
		DisplayRoomId.Text = room.RoomId;
		DisplayBetAmount.Text = room.BetAmount.ToString();

		// Hiển thị thông tin bản thân
		DisplayPlayerInfos[0].Display(room.Players[UserClass.Uid]);

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
		EventListener = new(
			FirebaseApi.BaseUrl, 
			$"Rooms/{room.RoomId}/Events", 
			UserClass.IdToken);

		EventListener.OnConnected += () => Console.WriteLine("Connected");
		EventListener.OnDisconnected += err => Console.WriteLine(err ? "Disconnected (error)" : "Stopped");
		EventListener.OnError += Console.WriteLine;

		EventListener.OnData += (eventType, json) =>
		{
			var evt = json.ToObject<MessageEvent>();

			if(evt != null && evt.data != null)
			{
				_ = Update(evt.data);
			}
		};		

		EventListener.Start();

		// Khởi tạo heartbeat service cho host
		heartbeatService = new HeartbeatService();
		AddChild(heartbeatService);
		heartbeatService.StartHeartbeat(room.RoomId, true); // Host
	}

	private async void OnLeaveRoomPressed()
	{
		LeaveRoom.Disabled = true;
		GD.Print($"Đang xóa phòng: {room.RoomId}");

		try
		{
			var deleteSuccess = await room.DeleteAsync();

			if (deleteSuccess.Item1)
			{
				GD.Print("Xóa phòng thành công!");
				
				EventListener.Stop();
				heartbeatService.StopHeartbeat();
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
			var newPlayer = await FirebaseApi.Get<PlayerClass>($"Rooms/{room.RoomId}/Players/{Pid}");
			await newPlayer.LoadAvatarAsync();
			newPlayer.Hands = new();
			room.Players.Add(Pid, newPlayer);

			// Lấy chỗ
			int newSeat = 1;
			while(room.Seats[newSeat] != null) newSeat++;

			room.Players[Pid].Seat = newSeat;
			room.Seats[newSeat] = Pid; // Xếp chỗ ngồi
			room.CurrentPlayers++;
			DisplayPlayerInfos[newSeat].Display(newPlayer);
			DisplayPlayerInfos[newSeat].Visible = true;

			GD.Print("Người chơi mới vào phòng.");
		}
			
		catch (Exception ex) {GD.PrintErr(ex.Message);}
	}

	private async void UpdateLeave(string Pid)
	{		
		// Lấy lại chỗ
		int CurrSeat = room.Players[Pid].Seat;		
		room.Seats[CurrSeat] = null;
		UnShowCards(Pid);
		DisplayPlayerInfos[CurrSeat].Visible = false;
		room.Players.Remove(Pid);
		room.CurrentPlayers--;

		// Xư lý nếu đang trong ván
		if (room.Status != "WAITING")
		{
			int playerTurnIndex = TurnOrder.IndexOf(Pid);
			TurnOrder.RemoveAt(playerTurnIndex);
			if (playerTurnIndex <= currentTurn)
			{
				currentTurn--;

				if(playerTurnIndex - 1 == currentTurn) // Đang đến lượt player đó
					UpdateStand(Pid); // Coi như đã stand
			}				
		}
	}
	
	//-------------------------Xử lý bắt đầu ván chơi----------------------------------------
	private async void OnStartGame()
	{
		if(room.CurrentPlayers < 2)
		{
			OS.Alert("Chưa đủ người để bắt đầu ván.");
			return;
		}

		bool started = await StartNewRound();
		if(started) StartGameButton.Disabled = true;
		else return;

		await DealCardInit();

		// Bắt đầu giai đoạn rút hoặc dằn
		await FirebaseApi.Put($"Rooms/{room.RoomId}/Status", "HIT_OR_STAND");
		room.Status = "HIT_OR_STAND";
		currentTurn = 0;
		HitOrStandPlayer(TurnOrder[currentTurn]);
	}

	private async Task<bool> StartNewRound()
	{
		try
		{
			await RoomEvent.PostRoomEventAsync(room.RoomId, "start_game");
			room.Status = "DEAL_INIT";

			for(int i = 1; i < 4; i++) // Không lấy cái
			{
				string pid = room.Seats[i];
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

			await RoomEvent.PostRoomEventAsync( // Ghi sự kiện
				room.RoomId, "deal_card", pid, card);

			int cardIndex = room.Players[pid].Hands.Count;
			room.Players[pid].Hands.Add(card); // Thêm trên dữ liệu			
			await FirebaseApi.Put($"Rooms/{room.RoomId}/Players/{pid}/Hands/{cardIndex}", card);

			int seat = room.Players[pid].Seat;
			if (room.Status == "HIT_OR_STAND")
				DisplayPlayerInfos[seat].StartCountdown(); // Chạy timer
			
			if(pid != UserClass.Uid)
			{
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
		await FirebaseApi.Put($"Rooms/{room.RoomId}/Status", "DEAL_INIT");
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
			await RoomEvent.PostRoomEventAsync
			(room.RoomId, "hit_or_stand", playerId);

			DisplayPlayerInfos[room.Players[playerId].Seat].StartCountdown();
		}

		catch (Exception ex) {GD.PrintErr(ex.Message);}      
	}

	private async void HitOrStandBookmaker()
	{
		var Score = room.Players[UserClass.Uid].CaclulateScore();

		while(Score.Item2 == 1 && Score.Item1 < 17) // Ngừng nếu trúng các trường hợp đặc biệt
		{
			await Task.Delay(500);
			await DealCard(UserClass.Uid);
			Score = room.Players[UserClass.Uid].CaclulateScore();
		}

		DisplayPlayerInfos[room.Players[UserClass.Uid].Seat].EndCountdown();
		ProcessResult(); // Bắt đầu tính điểm
	}

	private async void UpdateHit(string playerId)
	{
		DisplayPlayerInfos[room.Players[playerId].Seat].EndCountdown();
		await DealCard(playerId);				
	}

	private async void UpdateStand(string playerId)
	{
		if (room.Players.ContainsKey(playerId))
			DisplayPlayerInfos[room.Players[playerId].Seat].EndCountdown();
		currentTurn++;
		if(currentTurn < TurnOrder.Count)		
			HitOrStandPlayer(TurnOrder[currentTurn]); // Tiến đến player tiếp theo	

		else HitOrStandBookmaker();
	}	

	private async void ProcessResult() // Quá trình đánh giá kết quả
	{
		try
		{         
			await RoomEvent.PostRoomEventAsync(room.RoomId, "result");
			await FirebaseApi.Put($"Rooms/{room.RoomId}/Status", "RESULT");
			room.Status = "RESULT";

			(int, int) myScore = room.Players[UserClass.Uid].CaclulateScore();

			long betAmout = room.BetAmount;
			long myMoney = UserClass.Money;

			foreach (string pid in TurnOrder)
			{
				await Task.Delay(500);
				var pScore = room.Players[pid].CaclulateScore();
				long pMoney = room.Players[pid].Money;

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

				room.Players[pid].Money = pMoney;
				DisplayPlayerInfos[room.Players[pid].Seat].UpdateMoney(pMoney);

				await RoomEvent.PostRoomEventAsync(
					room.RoomId, result, pid);

				// Thêm animation chiến thắng, trường hợp đặc biệt, hiệu ứng cộng tiền
			}

			// Cập nhật:
			UserClass.Money = myMoney;
			room.Players[UserClass.Uid].Money = myMoney;
			DisplayPlayerInfos[0].UpdateMoney(myMoney);
			await FirebaseApi.Put($"Users/{UserClass.Uid}/Money", myMoney);
			await FirebaseApi.Put($"Rooms/{room.RoomId}/Players/{UserClass.Uid}/Money", myMoney);

			if (myMoney < (room.CurrentPlayers - 1) * 
			room.BetAmount)
				OnLeaveRoomPressed(); // Thoát và xóa phòng

			EndRound();
		}

		catch (Exception ex) {GD.PrintErr(ex.Message);}
	}

	private async void EndRound()
	{
		try
		{
			await RoomEvent.PostRoomEventAsync(room.RoomId, "end_round");	

			await Task.Delay(5000);

			room.Status = "WAITING";
			await FirebaseApi.Put(
				$"Rooms/{room.RoomId}/Status", "WATING");

			foreach (var p in room.Players)
			{
				foreach (var card in room.Players[p.Key].Hands)            
					DeckOfCards.Add(card); // Lấy lại bài
				UnShowCards(p.Key); // Ẩn bài
				await FirebaseApi.Delete(
					$"Rooms/{room.RoomId}/Players/{p.Key}/Hands");
				room.Players[p.Key].Hands.Clear();				
			}
			TurnOrder.Clear();

			await FirebaseApi.Put($"Rooms/{room.RoomId}/Status", "WAITING");
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
		int seat = room.Players[pid].Seat;

		for(int i = 0; i < room.Players[pid].Hands.Count; i++)        
			ShowCard(seat, i, room.Players[pid].Hands[i]);        
	}

	private void UnShowCards(string pid)
	{
		int seat = room.Players[pid].Seat;

		for(int i = 0; i < room.Players[pid].Hands.Count; i++)        
			DisplayCards[seat, i].Visible = false;
		DisplayCards[seat, 0].Frame = 52; // Mặt sau lá bài
	}
}
