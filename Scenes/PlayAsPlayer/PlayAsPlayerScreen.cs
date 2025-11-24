using Godot;
using System;
using NT106.Scripts.Models;
using System.Collections.Generic;
using CloudinaryDotNet;
using NT106.Scripts.Services;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Linq;

public partial class PlayAsPlayerScreen : Node2D
{
	public RoomClass room {get; set;}

	public List<TextureRect> DisplayAvatar {get; set;}
	public List<LineEdit> DisplayName {get; set;}
	public List<LineEdit> DisplayMoney {get; set;}
	public List<string> UidDisplayed {get; set;}


	public HashSet<int> AvilableSlot {get; set;} // Những chỗ còn trống

	// Hiển thị mã phòng và mức cược
	private LineEdit DisplayRoomId;
	private LineEdit DisplayBetAmount;

	// Rời phòng
	private Button LeaveRoom;

	// Lắng nghe realtime
	FirebaseStreaming EventListener;

	// Hiển thị các lá bài
	Sprite2D [,]DisplayCards;

	// Animation
	AnimationPlayer anim;

	public override void _Ready()
	{
		room = RoomClass.CurrentRoom;

		DisplayAvatar = new() {null, null, null, null};
		DisplayName = new() {null, null, null, null};
		DisplayMoney = new() {null, null, null, null};	
		UidDisplayed = new() {null, null, null, null};	

		// Hiển thị thông tin chung
		DisplayRoomId = GetNode<LineEdit>("pn_Background/le_RoomId");
		DisplayRoomId.Text = room.RoomId;
		DisplayBetAmount = GetNode<LineEdit>("pn_Background/le_BetAmount");
		DisplayBetAmount.Text = room.BetAmount.ToString();

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

		// Gán thông tin PLayer
		GetNodesForPlayers();

		// Hiển thị 
		UidDisplayed[0] = room.HostId;
		LoadAvatar(0, room.HostId); // Lấy Avatar trên Cloudinary
		room.Players[room.HostId].Seat = 0;
		Display(0, room.Players[room.HostId]);

		UidDisplayed[1] = UserClass.Uid;
		DisplayAvatar[1].Texture = UserClass.Avatar;
		Display(1, room.Players[UserClass.Uid]);

		int SeatIndex = 2;
		foreach(var p in room.Players)
		{
			if(p.Key != UserClass.Uid && p.Key != room.HostId)
			{
				UidDisplayed[SeatIndex] = p.Key;
				room.Players[p.Key].Seat = SeatIndex;
				LoadAvatar(SeatIndex, p.Key);
				Display(SeatIndex, room.Players[p.Key]);

				SeatIndex++;
			}
		}

		AvilableSlot = new();
		while(SeatIndex < 4)
		{
			AvilableSlot.Add(SeatIndex); // Lưu các vị trí trống
			SeatIndex++;
		} 

		// Thực hiện lắng nghe
		EventListener = new(FirebaseApi.BaseUrl, $"Rooms/{room.RoomId}/Events", UserClass.IdToken);
		EventListener.OnConnected += () => GD.Print("Firebase connected");
		EventListener.OnDisconnected += () => GD.Print("Firebase disconnected");
		EventListener.OnError += (msg) => GD.Print("ERR: " + msg);

		EventListener.OnData += (json) =>
		{
			var evt = json.ToObject<MessageEvent>();

			if(evt != null && evt.data != null)
			{
				_ = Update(evt.data.user, evt.data.type);	
			}
		};

		EventListener.Start();
	}

	private async void OnLeaveRoomPressed()
	{
		try
		{
			var res = await room.LeaveAsync();
			if(!res.Item1) throw new Exception(res.Item2);
			
			EventListener.Stop();
			GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");	
		}

		catch (Exception ex)
		{
			GD.PrintErr("Lỗi: ", ex.Message);
		}        
	}

	public void GetNodes(int Seat)
	{
		DisplayAvatar[Seat] = GetNode<TextureRect>($"pn_Background/ttr_Avatar_Player{Seat}");
		DisplayName[Seat] = GetNode<LineEdit>($"pn_Background/ttr_Avatar_Player{Seat}/le_InGameName_You");
		DisplayMoney[Seat] = GetNode<LineEdit>($"pn_Background/ttr_Avatar_Player{Seat}/le_Money");
	}
	public void GetNodesForPlayers()
	{
		for(int i = 0; i < 4; i++) GetNodes(i);
	}

	public void Display(int Seat, PlayerClass player)
	{
		DisplayAvatar[Seat].Visible = true;
		DisplayName[Seat].Text = player.InGameName;
		DisplayMoney[Seat].Text = player.Money.ToString();
	}

	public void UnDisplay(int Seat)
	{
		DisplayAvatar[Seat].Visible = false;
		DisplayAvatar[Seat].Texture = null;
		DisplayName[Seat].Text = null;
		DisplayMoney[Seat].Text = null;
	}

	public async void LoadAvatar(int Seat, string Uid)
	{
		DisplayAvatar[Seat].Texture = await CloudinaryService.GetImageAsync(Uid);
	}	

	private void OnError(Exception ex)
	{
		GD.PrintErr($"Firebase error: {ex.Message}");
	}

	private Task Update(string Pid, string Type)
	{
		if(Type == "join") CallDeferred(nameof(UpdateJoin), Pid);
		else if(Type == "leave") CallDeferred(nameof(UpdateLeave), Pid);
		else if(Type == "deleteRoom") CallDeferred(nameof(UpdataDelete));
		return Task.CompletedTask;
	}

	private async void UpdateJoin(string Pid)
	{
		var newPlayer = await FirebaseApi.Get<PlayerClass>($"Rooms/{room.RoomId}/Players/{Pid}.json?auth={UserClass.IdToken}");

		// Lấy chỗ
		int newSeat = AvilableSlot.First();
		AvilableSlot.Remove(newSeat);
		newPlayer.Seat = newSeat;
		room.Players.Add(Pid, newPlayer);

		// Hiển thị thông tin
		UidDisplayed[newSeat] = Pid;
		LoadAvatar(newSeat, Pid);
		Display(newSeat, newPlayer);
	}

	private void UpdateLeave(string Pid)
	{
		// Lấy lại chỗ
		int CurrSeat = room.Players[Pid].Seat;
		AvilableSlot.Add(CurrSeat);
		room.Players.Remove(Pid);
		UnDisplay(CurrSeat);
	}

	private void UpdataDelete()
	{
		OS.Alert("Phòng đã bị xoá bởi chủ phòng!");
		EventListener.Stop();
		GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");	
	}

	// Show giá trị lá bài
	private void DisplayCard(int playerIndex, int cardIndex, (int, int) card)
	{
		DisplayCards[playerIndex, cardIndex].Frame = card.Item1 * 13 + card.Item2;
		DisplayCards[playerIndex, cardIndex].Visible = true;
	}

	// Chia bài cho bạn
	private void AnimDealYou(int cardIndex, (int, int) card) // CardIndex: Lá bài thứ mấy
	{
		anim.Play($"DealCard{cardIndex}");
		DisplayCard(1, cardIndex, card);
		anim.Queue("RESET"); // Quay về trạng thái ban đầu
	}

	// Chia bài cho 1 player (1 đến 3)
	private void AnimDealPlayer(int playerIndex, (int, int) card)
	{
		anim.Play($"DealPlayer{playerIndex}");
		DisplayCards[playerIndex, 0].Visible = true;
		anim.Queue("RESET");
	}
}
