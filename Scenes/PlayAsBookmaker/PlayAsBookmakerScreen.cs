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
		if(evtData.type == "join") CallDeferred(nameof(UpdateJoin), evtData.user);
		else if(evtData.type == "leave") CallDeferred(nameof(UpdateLeave), evtData.user);
		return Task.CompletedTask;
	}

	private async void UpdateJoin(string Pid)
	{
		try
		{
			var newPlayer = await FirebaseApi.Get<PlayerClass>($"Rooms/{RoomClass.CurrentRoom.RoomId}/Players/{Pid}.json?auth={UserClass.IdToken}");
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
			
		catch (Exception ex)
		{
			GD.PrintErr(ex.Message);
		}
	}

	private void UpdateLeave(string Pid)
	{
		// Lấy lại chỗ
		int CurrSeat = RoomClass.CurrentRoom.Players[Pid].Seat;
		RoomClass.CurrentRoom.Players.Remove(Pid);
		RoomClass.CurrentRoom.Seats[CurrSeat] = null;
		DisplayPlayerInfos[CurrSeat].Visible = false;
	}

	
	//-------------------------Xử lý bắt đầu trò chơi----------------------------------------
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
	}

	private async Task<bool> StartNewRound()
	{
		try
		{
			var startEvt = new RoomEvent
			{
				type = "start_game",
			};

			//Ghi event lên firebase
			await FirebaseApi.Post($"Rooms/{RoomClass.CurrentRoom.RoomId}/Events.json?auth={UserClass.IdToken}", startEvt);

			RoomClass.CurrentRoom.Status = "PLAYING";
			//cập nhật lên firebase
			await FirebaseApi.Patch($"Rooms/{RoomClass.CurrentRoom.RoomId}/Status.json?auth={UserClass.IdToken}", "PLAYING");

			for(int i = 1; i < 4; i++)
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
				payload = card
			};

			await FirebaseApi.Post($"Rooms/{RoomClass.CurrentRoom.RoomId}/Events.json?auth={UserClass.IdToken}", DealEvt);
			
			if(pid != UserClass.Uid)
			{
				int seat = RoomClass.CurrentRoom.Players[pid].Seat;
				anim.Play($"DealPlayer{seat}");
				DisplayCards[seat, 0].Visible = true;			
			}

			else
			{
				int cardIndex = RoomClass.CurrentRoom.Players[pid].Hands.Count;
				anim.Play($"DealCard{cardIndex}");
				DisplayCards[0, cardIndex].Frame = (card.Item1 - 1) + (card.Item2 - 1) * 13;
				DisplayCards[0, cardIndex].Visible = true;
			}
			anim.Queue("RESET");

			RoomClass.CurrentRoom.Players[pid].Hands.Add(card); // Thêm trên dữ liệu
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
				await DealCard(pid);	
	}
}
