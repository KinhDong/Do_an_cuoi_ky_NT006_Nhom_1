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
	private FirebaseStream lisEvent, lisDelete;

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
		lisEvent = new();
		AddChild(lisEvent);
		string EventUrl = $"{FirebaseApi.BaseUrl}/Rooms/{room.RoomId}/Events.json?auth={UserClass.IdToken}";
		lisEvent.StartListen(EventUrl, OnRoomData, OnError);

		lisDelete = new();
		AddChild(lisDelete);
		string DeleteUrl = $"{FirebaseApi.BaseUrl}/Rooms/{room.RoomId}.json?auth={UserClass.IdToken}";
		lisDelete.StartListen(DeleteUrl, 
		data =>
		{
			if(data == null) OnRoomDelete();
		});
	}

	private async void OnLeaveRoomPressed()
	{
		try
		{
			var res = await room.LeaveAsync();
			if(!res.Item1) throw new Exception(res.Item2);

			lisEvent.StopListen();
			lisDelete.StopListen();
			GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");	
		}

		catch (Exception ex)
		{
			GD.PrintErr("Lỗi: ", ex.Message);
		}        
	}

	public override void _ExitTree()
	{
		lisEvent?.StopListen();
		lisDelete?.StopListen();
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
		DisplayAvatar[Seat] = null;
		DisplayName[Seat] = null;
		DisplayMoney[Seat] = null;
		DisplayAvatar[Seat].Visible = false;
	}

	public async void LoadAvatar(int Seat, string Uid)
	{
		DisplayAvatar[Seat].Texture = await CloudinaryService.GetImageAsync(Uid);
	}

	private void OnRoomData(string json)
	{
		var evt = JsonConvert.DeserializeObject<RoomEvent>(json);

		switch(evt.type)
		{
			case "join": 
				UpdateJoin(evt.user);
				break;

			case "leave":
				UpdataLeave(evt.user);
				break;

		}
	}

	private void OnError(Exception ex)
	{
		GD.PrintErr($"Firebase error: {ex.Message}");
	}

	private async void UpdateJoin(string Pid)
	{
		var newPlayer = await FirebaseApi.Get<PlayerClass>($"Rooms/{room.RoomId}/{Pid}.json?auth={UserClass.IdToken}");

		room.Players.Add(Pid, newPlayer);

		// Lấy chỗ
		int newSeat = AvilableSlot.First();
		AvilableSlot.Remove(newSeat);

		// Hiển thị thông tin
		UidDisplayed[newSeat] = Pid;
		LoadAvatar(newSeat, Pid);
		Display(newSeat, newPlayer);
	}

	private void UpdataLeave(string Pid)
	{
		// Lấy lại chỗ
		int CurrSeat = room.Players[Pid].Seat;
		AvilableSlot.Add(CurrSeat);
		room.Players.Remove(Pid);
		UnDisplay(CurrSeat);
	}

	private void OnRoomDelete()
	{
		GD.Print("Phòng đã bị xóa");

		lisEvent.StopListen();
		lisDelete.StopListen();

		RoomClass.CurrentRoom = null;
		GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");
	}
}
