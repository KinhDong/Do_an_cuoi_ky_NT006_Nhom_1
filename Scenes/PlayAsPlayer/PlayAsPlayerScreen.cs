using Godot;
using Godot.Collections;
using System;
using NT106.Scripts.Models;
using NT106.Scripts.Services;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public partial class PlayAsPlayerScreen : Node2D
{
	// Hiển thị mã phòng và mức cược
	[Export] private LineEdit DisplayRoomId;
	[Export] private LineEdit DisplayBetAmount;	
	[Export] private Button LeaveRoom; // Rời phòng

	[Export] Array<DisplayPlayerInfo> DisplayPlayerInfos;

	// Nút Hit/Stand
	[Export] private Button HitButton;
	[Export] private Button StandButton;

	// Lắng nghe realtime
	FirebaseStreaming EventListener;
	string CurrTime;

	// ----- Ván chơi

	// Hiển thị các lá bài
	Sprite2D[,] DisplayCards;

	// Animation
	[Export] AnimationPlayer anim;

	// Biến để theo dõi lượt chơi hiện tại
	private string currentPlayerTurn;

	public override void _Ready()
	{
		// Hiển thị thông tin chung
		DisplayRoomId.Text = RoomClass.CurrentRoom.RoomId;
		DisplayBetAmount.Text = RoomClass.CurrentRoom.BetAmount.ToString();

		// Rời phòng
		LeaveRoom = GetNode<Button>("pn_Background/btn_LeaveRoom");
		LeaveRoom.Pressed += OnLeaveRoomPressed;

		HitButton.Pressed += OnHitPressed;
		StandButton.Pressed += OnStandPressed;

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
		foreach (var p in RoomClass.CurrentRoom.Players) 
			DisplayPlayerInfos[p.Value.Seat].Display(p.Value);

		// Hiển thị các lá bài hiện tại (Nếu phòng trong trạng thái đang chơi)
		if (RoomClass.CurrentRoom.Status == "PLAYING")
		{
			for(int i = 0; i < 4; i++)
			{
				string pid = RoomClass.CurrentRoom.Seats[i];
				if(pid == null) continue;
				if(RoomClass.CurrentRoom.Players[pid].Hands.Count > 0)
					DisplayCards[i, 0].Visible = true;
			}
		}        

		// Thực hiện lắng nghe
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

		CurrTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"); // Thời gian hiện tại
	}

	private async void OnLeaveRoomPressed()
	{
		try
		{
			var res = await RoomClass.CurrentRoom.LeaveAsync();
			if(!res.Item1) throw new Exception(res.Item2);
			
			EventListener.Stop();
			RoomClass.CurrentRoom = null;
			GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");	
		}

		catch (Exception ex)
		{
			GD.PrintErr("Lỗi: ", ex.Message);
		}        
	}

	private async void UpdateJoin(string Pid)
	{
		var newPlayer = await FirebaseApi.Get<PlayerClass>($"Rooms/{RoomClass.CurrentRoom.RoomId}/Players/{Pid}");
		await newPlayer.LoadAvatarAsync();
		newPlayer.Hands = new();
		RoomClass.CurrentRoom.Players.Add(Pid, newPlayer);

		int seat = 2;
		while(RoomClass.CurrentRoom.Seats[seat] != null) seat++;
		
		RoomClass.CurrentRoom.Players[Pid].Seat = seat;
		DisplayPlayerInfos[seat].Display(RoomClass.CurrentRoom.Players[Pid]);
		DisplayPlayerInfos[seat].Visible = true;
	}

	private void UpdateLeave(string Pid)
	{
		// Lấy lại chỗ
		int seat = RoomClass.CurrentRoom.Players[Pid].Seat;
		RoomClass.CurrentRoom.Players.Remove(Pid);
		RoomClass.CurrentRoom.Seats[seat] = null;
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
		RoomClass.CurrentRoom.Status = "PLAYING";
		OS.Alert("Ván mới đã bắt đầu!");
	}

	private void UpdateDeal(string pid, int rank, int suit)
	{	
		RoomClass.CurrentRoom.Players[pid].Hands.Add((rank, suit));

		if(pid != UserClass.Uid)
		{
			int seat = RoomClass.CurrentRoom.Players[pid].Seat;
			anim.Play($"DealPlayer{seat}");
			anim.Queue("RESET");
			DisplayCards[seat, 0].Visible = true;
		}

		else
		{
			int cardIndex = RoomClass.CurrentRoom.Players[pid].Hands.Count - 1;
			GD.Print(cardIndex);
			anim.Play($"DealCard{cardIndex}");
			anim.Queue("RESET");
			ShowCard(1, cardIndex, (rank, suit));
			DisplayCards[1, cardIndex].Visible = true;
			if (cardIndex >= 2) UpdateHitOrStand(UserClass.Uid);
		}
	}

	private async void UpdateHitOrStand(string playerId)
	{
		// Animation làm sáng các thứ

		if (playerId != UserClass.Uid) return; // Không phải mình thì không quan tâm

		var score = RoomClass.CurrentRoom.Players[playerId].CaclulateScore();
		GD.Print($"Score: {score.Item1}, Strength: {score.Item2}");

		if(score.Item2 != 1)
		{
			StandButton.Disabled = false;
			return;
		}

		if(score.Item1 < 16)
		{
			HitButton.Disabled = false;
			return;
		}

		HitButton.Disabled = false;
		StandButton.Disabled = false;
	}

	private async void OnHitPressed()
	{
		try
		{
			HitButton.Disabled = true;
			StandButton.Disabled = true;
			var evt = new RoomEvent
			{
				type = "hit",
				user = UserClass.Uid,
				time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
			};
			await FirebaseApi.Post($"Rooms/{RoomClass.CurrentRoom.RoomId}/Events", evt);
		}

		catch (Exception ex)
		{
			GD.PrintErr(ex.Message);
		}		
	}

	private async void OnStandPressed()
	{
		try
		{
			HitButton.Disabled = true;
			StandButton.Disabled = true;
			var evt = new RoomEvent
			{
				type = "stand",
				user = UserClass.Uid,
				time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
			};
			await FirebaseApi.Post($"Rooms/{RoomClass.CurrentRoom.RoomId}/Events", evt);			
		}
		
		catch (Exception ex)
		{
			GD.PrintErr(ex.Message);
		}
	}

	private void UpdateStand(string playerId)
	{
		// Không làm sáng nữa
	}	

	private async void UpdateResultProcess()
	{
		ShowCards(RoomClass.CurrentRoom.HostId); // Show bài của Cái
	}

	private async void UpdateResult(string pid, string result)
	{
		try
		{			
			if (pid != UserClass.Uid) ShowCards(pid); // Show bài của Player

			if (result == "draw")
			{
				// Animation các thứ
				return;
			}

			long betAmout = RoomClass.CurrentRoom.BetAmount;
			long currMoney = RoomClass.CurrentRoom.Players[pid].Money;

			if (result == "win")
			{
				// Animation
				currMoney += betAmout;
			}
			else
			{
				// Animation
				currMoney -= betAmout;
			} 

			RoomClass.CurrentRoom.Players[pid].Money = currMoney;
			DisplayPlayerInfos[RoomClass.CurrentRoom.Players[pid].Seat].UpdateMoney(currMoney);
			if (pid == UserClass.Uid)
				await FirebaseApi.Put($"Users/{UserClass.Uid}/Money", currMoney);
		}

		catch (Exception ex) {GD.PrintErr(ex.Message);}
	}

	private async void UpdateEnd()
	{
		await Task.Delay(5000); // Cho 5s ngắm bài
		foreach (var player in RoomClass.CurrentRoom.Players)
		{
			UnShowCards(player.Key);
			RoomClass.CurrentRoom.Players[player.Key].Hands.Clear();		
		}
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
		if (pid != UserClass.Uid)
			DisplayCards[seat, 0].Visible = true;
	}
}
