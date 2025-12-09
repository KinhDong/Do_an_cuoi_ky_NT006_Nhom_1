using Godot;
using Godot.Collections;
using System;
using NT106.Scripts.Models;
using NT106.Scripts.Services;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public partial class PlayAsPlayerScreen : Node2D
{
	// Hiển thị mã phòng và mức cược
	[Export] private LineEdit DisplayRoomId;
	[Export] private LineEdit DisplayBetAmount;	
	[Export] private Button LeaveRoom; // Rời phòng

	[Export] Array<DisplayPlayerInfo> DisplayPlayerInfos;

	// Lắng nghe realtime
	FirebaseStreaming EventListener;

	// ----- Ván chơi

	// Hiển thị các lá bài
	Sprite2D[,] DisplayCards;

	// Animation
	[Export] AnimationPlayer anim;

	public override void _Ready()
	{
		// Hiển thị thông tin chung
		DisplayRoomId.Text = RoomClass.CurrentRoom.RoomId;
		DisplayBetAmount.Text = RoomClass.CurrentRoom.BetAmount.ToString();

		// Rời phòng
		LeaveRoom = GetNode<Button>("pn_Background/btn_LeaveRoom");
		LeaveRoom.Pressed += OnLeaveRoomPressed;

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
		if(evtData.type == "join") CallDeferred(nameof(UpdateJoin), evtData.user);
		else if(evtData.type == "leave") CallDeferred(nameof(UpdateLeave), evtData.user);
		else if(evtData.type == "deleteRoom") CallDeferred(nameof(UpdateDelete));
		else if(evtData.type == "start_game") CallDeferred(nameof(UpdateStartGame));
		else if(evtData.type == "deal_card") {
			JObject obj = (JObject)evtData.payload;
			int rank = obj.Value<int>("Item1");
			int suit = obj.Value<int>("Item2");
			CallDeferred(nameof(UpdateDeal), evtData.user, rank, suit);
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
			DisplayCards[1, cardIndex].Frame = (rank - 1) * (suit - 1) * 13;
			DisplayCards[1, cardIndex].Visible = true;
        }
		anim.Queue("RESET");

		RoomClass.CurrentRoom.Players[pid].Hands.Add((rank, suit));
    }
}
