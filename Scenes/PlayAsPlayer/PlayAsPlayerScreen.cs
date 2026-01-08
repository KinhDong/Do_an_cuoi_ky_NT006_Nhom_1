using Godot;
using Godot.Collections;
using System;
using NT106.Scripts.Models;
using NT106.Scripts.Services;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public partial class PlayAsPlayerScreen : Node2D
{
	RoomClass room;
	// Hiển thị mã phòng và mức cược
	[Export] private LineEdit DisplayRoomId;
	[Export] private LineEdit DisplayBetAmount;	
	[Export] private Button LeaveRoom; // Rời phòng

	[Export] Array<DisplayPlayerInfo> DisplayPlayerInfos;
	[Export] StartEffect startEffect;
	[Export] Button SettingButton;

	// Nút Hit/Stand
	[Export] private Button HitButton;
	[Export] private Button StandButton;

	// Lắng nghe realtime
	FirebaseStreaming EventListener;
	string CurrTime;

	HeartbeatService heartbeatService;

	// ----- Ván chơi

	// Hiển thị các lá bài
	Sprite2D[,] DisplayCards;

	// Animation
	[Export] AnimationPlayer anim;

	// Timer cho lựa chọn Hit/Stand
	[Export] private Timer timer;

	// Điểm số hiện tại để quyết định khi timeout
	private (int, int) currentScore;

	//nhạc nền
	[Export] public AudioStream BackgroundMusic;

	//âm thanh hiệu ứng 
	[Export] public AudioStream sfxClick;
	[Export] public AudioStream sfxDealCard;
	[Export] public AudioStream sfxWin;
	[Export] public AudioStream sfxLose;
	[Export] public AudioStream sfxStartGame;
	public override void _Ready()
	{
		room = RoomClass.CurrentRoom;
		GD.Print("Room loaded: ", room?.RoomId ?? "null");
		// Hiển thị thông tin chung
		DisplayRoomId.Text = room.RoomId;
		DisplayBetAmount.Text = room.BetAmount.ToString();
		// Phát nhạc nền
		if (BackgroundMusic != null)
		{
			AudioManager.Instance.PlayMusic(BackgroundMusic);
		}

		// Rời phòng
		LeaveRoom = GetNode<Button>("pn_Background/btn_LeaveRoom");
		GD.Print("LeaveRoom node: ", LeaveRoom != null ? "found" : "null");
		if (LeaveRoom != null)
		{
			GD.Print("LeaveRoom visible: ", LeaveRoom.Visible, " disabled: ", LeaveRoom.Disabled);
			LeaveRoom.Disabled = false;
			LeaveRoom.Visible = true;
		}
		LeaveRoom.Pressed += OnLeaveRoomPressed;

		// Setting
		SettingButton.Pressed += () =>
		{
			AudioManager.Instance.PlaySFX(sfxClick);
			var settingScene = ResourceLoader.Load<PackedScene>("res://Scenes/SettingScenes/SettingScenes.tscn");
			var settingInstance = settingScene.Instantiate();
			AddChild(settingInstance);
		};

		HitButton.Pressed += OnHitPressed;
		StandButton.Pressed += OnStandPressed;

		timer.Timeout += OnTimerTimeout;

		// Gán tiếng click cho các nút
		HitButton.Pressed += () => AudioManager.Instance.PlaySFX(sfxClick);
		StandButton.Pressed += () => AudioManager.Instance.PlaySFX(sfxClick);
		LeaveRoom.Pressed += () => AudioManager.Instance.PlaySFX(sfxClick);

		// Hiển thị các lá bài
		DisplayCards = new Sprite2D[4, 5];
		for(int i = 0; i < 4; i++)
		{
			for(int j = 0; j < 5; j++)
			{
				DisplayCards[i, j] = GetNode<Sprite2D>($"pn_Background/ttr_Table/CardsOfPlayer{i}/Card{j}");
			}
		}

		// Hiển thị thông tin các người chơi
		foreach (var p in room.Players) 
		{
			GD.Print("Displaying player: ", p.Value.InGameName, " at seat ", p.Value.Seat, " avatar: ", p.Value.Avatar != null ? "loaded" : "null");
			if (DisplayPlayerInfos[p.Value.Seat] == null)
			{
				GD.PrintErr("DisplayPlayerInfo is null at seat ", p.Value.Seat);
				continue;
			}
			DisplayPlayerInfos[p.Value.Seat].Display(p.Value);
			GD.Print("Set visible for seat ", p.Value.Seat);
		}

		// Hiển thị các lá bài của những người chơi đã ở trong phòng
		if (room.Status != "WAITING")
		{
			for(int i = 0; i < 4; i++)
			{
				string pid = room.Seats[i];
				if(pid == null) continue;
				if(room.Players[pid].Hands.Count > 0)
					DisplayCards[i, 0].Visible = true;
			}
		}        

		// Thực hiện lắng nghe
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

		CurrTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"); // Thời gian hiện tại

		// Khởi tạo heartbeat service cho player
		heartbeatService = new HeartbeatService();
		AddChild(heartbeatService);
		heartbeatService.StartHeartbeat(room.RoomId, false); // Not host
	}

	private async void OnLeaveRoomPressed()
	{
		GD.Print("Leave button pressed");
		try
		{
			var res = await room.LeaveAsync();
			GD.Print("Leave result: ", res.Item1, " ", res.Item2);
			if(!res.Item1) throw new Exception(res.Item2);
			
			EventListener.Stop();
			heartbeatService.StopHeartbeat();
			RoomClass.CurrentRoom = null;
			GetTree().ChangeSceneToFile("res://Scenes/CreateOrJoinRoomScreen/CreateOrJoinRoom.tscn");	
		}

		catch (Exception ex)
		{
			GD.PrintErr("Lỗi: ", ex.Message);
		}        
	}

	private async void UpdateJoin(string Pid)
	{
		var newPlayer = await FirebaseApi.Get<PlayerClass>($"Rooms/{room.RoomId}/Players/{Pid}");
		await newPlayer.LoadAvatarAsync();
		newPlayer.Hands = new();
		room.Players.Add(Pid, newPlayer);

		int seat = 2;
		while(room.Seats[seat] != null) seat++;
		
		room.Players[Pid].Seat = seat;
		DisplayPlayerInfos[seat].Display(room.Players[Pid]);
		DisplayPlayerInfos[seat].Visible = true;
	}

	private void UpdateLeave(string Pid)
	{
		// Lấy lại chỗ
		int seat = room.Players[Pid].Seat;
		room.Players.Remove(Pid);
		room.Seats[seat] = null;
		DisplayPlayerInfos[seat].Visible = false;
	}

	private void UpdateDelete()
	{
		OS.Alert("Phòng đã bị xoá bởi chủ phòng!");
		EventListener.Stop();
		RoomClass.CurrentRoom = null;
		GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");	
	}

	//-------------------------Xử lý bắt đầu ván chơi----------------------------------------

	private Task Update(RoomEvent evtData)
	{
		if (evtData.time.CompareTo(CurrTime) < 0)
			return Task.CompletedTask; // Không xem các sự kiện cũ hơn

		switch (evtData.type)
		{
			case "join": 
				CallDeferred(nameof(UpdateJoin), evtData.user);
				break;

			case "leave":
				CallDeferred(nameof(UpdateLeave), evtData.user);
				break;

			case "delete_room":
				CallDeferred(nameof(UpdateDelete));
				break;

			case "start_game":
				CallDeferred(nameof(UpdateStartGame));
				break;
			
			case "deal_card":
				JObject obj = (JObject)evtData.payload;
				int rank = obj.Value<int>("Item1");
				int suit = obj.Value<int>("Item2");
				CallDeferred(nameof(UpdateDeal), evtData.user, rank, suit);
				break;

			case "hit_or_stand":
				CallDeferred(nameof(UpdateHitOrStand), evtData.user);
				break;

			case "hit":
				CallDeferred(nameof(UpdateHit), evtData.user);
				break;

			case "stand":
				CallDeferred(nameof(UpdateStand), evtData.user);
				break;

			case "result":
				CallDeferred(nameof(UpdateResultProcess));
				break;

			case "win" or "lose" or "draw":
				CallDeferred(nameof(UpdateResult), evtData.user, evtData.type);
				break;

			case "end_round":
				CallDeferred(nameof(UpdateEnd));
				break;

			default:
				break;
		}

		return Task.CompletedTask;
	}

	private void UpdateStartGame()
	{
		room.Status = "DEAL_INIT";
		startEffect.Visible = true;
		startEffect.startBanner();
	}

	private async void UpdateDeal(string pid, int rank, int suit)
	{	
		int cardIndex = room.Players[pid].Hands.Count;
		room.Players[pid].Hands.Add((rank, suit));
		int seat = room.Players[pid].Seat;

		AudioManager.Instance.PlaySFX(sfxDealCard);// Phát âm thanh chia bài
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
			ShowCard(1, cardIndex, (rank, suit));			
			DisplayCards[1, cardIndex].Visible = true;
			if (cardIndex >= 2) UpdateHitOrStand(UserClass.Uid);
		}

		if (room.Status == "HIT_OR_STAND")
			DisplayPlayerInfos[seat].StartCountdown();
		anim.Queue("RESET");
	}

	private async void UpdateHitOrStand(string playerId)
	{
		room.Status = "HIT_OR_STAND";
		// Animation làm sáng các thứ
		DisplayPlayerInfos[room.Players[playerId].Seat].StartCountdown();
		timer.Start(10);

		if (playerId != UserClass.Uid) return; // Không phải mình thì không quan tâm

		var score = room.Players[playerId].CaclulateScore();
		currentScore = score;
		GD.Print($"Score: {score.Item1}, Strength: {score.Item2}");

		int seat = RoomClass.CurrentRoom.Players[playerId].Seat;
		DisplayPlayerInfos[seat].HighlightPlayerTurn();

		if(score.Item2 != 1)
		{
			StandButton.Disabled = false;
			timer.Start(10);
			return;
		}

		if(score.Item1 < 16)
		{
			HitButton.Disabled = false;
			timer.Start(10);
			return;
		}

		HitButton.Disabled = false;
		StandButton.Disabled = false;		
	}

	private async void OnHitPressed()
	{
		timer.Stop();
		HitButton.Disabled = true;
		StandButton.Disabled = true;

		await RoomEvent.PostRoomEventAsync(room.RoomId, "hit", UserClass.Uid);	
	}

	private async void OnStandPressed()
	{
		timer.Stop();
		HitButton.Disabled = true;
		StandButton.Disabled = true;	

		await RoomEvent.PostRoomEventAsync(room.RoomId, "stand", UserClass.Uid);	
	}

	private void UpdateHit(string pid)
	{
		DisplayPlayerInfos[room.Players[pid].Seat].EndCountdown();
	}

	private void UpdateStand(string pid)
	{
		// Không làm sáng nữa
		int seat = RoomClass.CurrentRoom.Players[pid].Seat;
		DisplayPlayerInfos[seat].NotHighlightPlayerTurn();
		DisplayPlayerInfos[room.Players[pid].Seat].EndCountdown();
	}	

	private async void UpdateResultProcess()
	{
		room.Status = "RESULT";
		DisplayPlayerInfos[0].EndCountdown();
		ShowCards(room.HostId); // Show bài của Cái
	}

	private async void UpdateResult(string pid, string result)
	{
		try
		{			
			if (pid != UserClass.Uid) ShowCards(pid); // Show bài của Player

			int betAmout = room.BetAmount;
			int change = 0;
			int pSeat = room.Players[pid].Seat;

			if (result == "win")
			{
				// Animation
				AudioManager.Instance.PlaySFX(sfxWin);// Phát âm thanh thắng
				DisplayPlayerInfos[pSeat].AddMoneyEffect(betAmout);
				DisplayPlayerInfos[0].MinusMoneyEffect(betAmout);
				change = betAmout;
			}
			else if (result == "lose")
			{
				// Animation
				AudioManager.Instance.PlaySFX(sfxLose); // Phát âm thanh thua
				DisplayPlayerInfos[pSeat].MinusMoneyEffect(betAmout);
				DisplayPlayerInfos[0].AddMoneyEffect(betAmout);
				change = - betAmout;
			} 
			room.Players[room.HostId].Money -= change;
			DisplayPlayerInfos[0].UpdateMoney(
				room.Players[room.HostId].Money);

			room.Players[pid].Money += change;
			DisplayPlayerInfos[pSeat].UpdateMoney(
				room.Players[pid].Money);			

			if (pid == UserClass.Uid)
			{
				UserClass.Money += change;
				await FirebaseApi.Put($"Users/{UserClass.Uid}/Money", UserClass.Money);
				await FirebaseApi.Put($"Rooms/{room.RoomId}/Players/{UserClass.Uid}/Money", UserClass.Money);
			
				// Thêm lịch sử kết quả
				var playerResult = new PlayerResult
				{
					PlayerInGameName = UserClass.InGameName,
					Role = "Người chơi",
					Score = room.Players[pid].CaclulateScore().Item1,
					Strength = room.Players[pid].CaclulateScore().Item2,
					Result = result,
					MoneyChange = change
				};

				var bookmakerResult = new PlayerResult
				{
					PlayerInGameName = room.Players[room.HostId].InGameName,
					Role = "Cái",
					Score = room.Players[room.HostId].CaclulateScore().Item1,
					Strength = room.Players[room.HostId].CaclulateScore().Item2,
					Result = result == "win" ? "lose" : result == "lose" ? "win" : "draw",
					MoneyChange = change
				};

				var matchHistory = new MatchHistory
				{
					Datetime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"),
					RoomId = room.RoomId,
					You = playerResult,
					Opponent = bookmakerResult
				};

				await FirebaseApi.Post($"Users/{UserClass.Uid}/MatchHistories", matchHistory);
			}				
		}

		catch (Exception ex) {GD.PrintErr(ex.Message);}
	}

	private async void UpdateEnd()
	{
		await Task.Delay(5000); // Cho 5s ngắm bài
		foreach (var player in room.Players)
		{
			UnShowCards(player.Key);
			room.Players[player.Key].Hands.Clear();		
		}

		room.Status = "WAITING";
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

	private async void OnTimerTimeout()
	{
		HitButton.Disabled = true;
		StandButton.Disabled = true;

		if (currentScore.Item1 < 16)
			OnHitPressed();			
		else
			OnStandPressed();	
	}
}
