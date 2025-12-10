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
	}

	private async void OnLeaveRoomPressed()
	{
		try
		{
			var res = await RoomClass.CurrentRoom.LeaveAsync();
			if(!res.Item1) throw new Exception(res.Item2);
			
			EventListener.Stop();
			GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");	
		}

		catch (Exception ex)
		{
			GD.PrintErr("Lỗi: ", ex.Message);
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

			case "deleteRoom":
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

			default:
				break;
        }

		return Task.CompletedTask;
	}

	private async void UpdateJoin(string Pid)
	{
		var newPlayer = await FirebaseApi.Get<PlayerClass>($"Rooms/{RoomClass.CurrentRoom.RoomId}/Players/{Pid}.json?auth={UserClass.IdToken}");
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
		RoomClass.CurrentRoom.Seats[seat] = null;
		DisplayPlayerInfos[seat].Visible = false;
	}

	private void UpdateDelete()
	{
		OS.Alert("Phòng đã bị xoá bởi chủ phòng!");
		EventListener.Stop();
		GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");	
	}

	private void UpdateStartGame()
	{
		RoomClass.CurrentRoom.Status = "PLAYING";
		OS.Alert("Ván mới đã bắt đầu!");
	}

	private void UpdateDeal(string pid, int rank, int suit)
    {		
        if(pid != UserClass.Uid)
        {
			int seat = RoomClass.CurrentRoom.Players[pid].Seat;
            anim.Play($"DealPlayer{seat}");
			DisplayCards[seat, 0].Visible = true;
        }

		else
        {
            int cardIndex = RoomClass.CurrentRoom.Players[pid].Hands.Count;
			GD.Print(cardIndex);
			anim.Play($"DealCard{cardIndex}");
			DisplayCards[1, cardIndex].Frame = (rank - 1) + (suit - 1) * 13;
			DisplayCards[1, cardIndex].Visible = true;
        }
		anim.Queue("RESET");

		RoomClass.CurrentRoom.Players[pid].Hands.Add((rank, suit));

		if (RoomClass.CurrentRoom.Players[pid].Hands.Count > 2) UpdateHitOrStand(UserClass.Uid);
    }

	private async void UpdateHitOrStand(string playerId)
    {
        // Animation làm sáng các thứ

		if (playerId != UserClass.Uid) return; // Không phải mình thì không quan tâm

		int score = CalculateScore(RoomClass.CurrentRoom.Players[playerId].Hands);
		int cardCount = RoomClass.CurrentRoom.Players[playerId].Hands.Count;

		if(RoomClass.CurrentRoom.Players[playerId].Hands.Count == 5 || score >= 21)
        {
            OnStandPressed(); // Buộc dừng
			return;
        }

		if(score < 16)
        {
            OnHitPressed(); // Buộc rút
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
			var evt = new RoomEvent
			{
				type = "hit",
				user = UserClass.Uid
			};
			await FirebaseApi.Post(
				$"Rooms/{RoomClass.CurrentRoom.RoomId}/Events.json?auth={UserClass.IdToken}", evt);
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
				user = UserClass.Uid
			};
			await FirebaseApi.Post(
				$"Rooms/{RoomClass.CurrentRoom.RoomId}/Events.json?auth={UserClass.IdToken}", evt);
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


	// Tính điểm
	private int CalculateScore(List<(int, int)> cards)
	{
	   	int score = 0;
		int aceCount = 0;
	
		foreach (var card in cards)
		{
			var rank = card.Item1;
		
			if (rank == 1) // Ace
			{
				aceCount++;
				score += 1;
			}
			else if (rank >= 10) // 10, J, Q, K
			{
				score += 10;
			}
			else
			{
				score += rank;
			}
		}
	
		return score; 
	}
}
