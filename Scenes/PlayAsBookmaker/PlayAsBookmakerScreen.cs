using Godot;
using System;
using NT106.Scripts.Models;
using System.Collections.Generic;
using NT106.Scripts.Services;
using Newtonsoft.Json;
using System.Linq;

public partial class PlayAsBookmakerScreen : Node2D
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

	FirebaseStream lisEvent;

	public override void _Ready()
	{
		room = RoomClass.CurrentRoom;

		DisplayAvatar = new() {null, null, null, null};
		DisplayName = new() {null, null, null, null};
		DisplayMoney = new() {null, null, null, null};

		// Tạo các vị trí trống
		AvilableSlot = new HashSet<int> {1, 2, 3};			

		// Hiển thị thông tin chung
		DisplayRoomId = GetNode<LineEdit>("pn_Background/le_RoomId");
		DisplayRoomId.Text = room.RoomId;
		DisplayBetAmount = GetNode<LineEdit>("pn_Background/ttr_Table/le_BetAmount");
		DisplayBetAmount.Text = room.BetAmount.ToString();

		// Gán và hiển thị thông tin PLayer
		GetNodesForPlayers();
		DisplayAvatar[0].Texture = UserClass.Avatar;
		Display(0, room.Players[room.HostId]);


		LeaveRoom = GetNode<Button>("pn_Background/btn_LeaveRoom");
		LeaveRoom.Pressed += OnLeaveRoomPressed;

		// Lắng nghe
		lisEvent = new();
		AddChild(lisEvent);
		string EventUrl = $"{FirebaseApi.BaseUrl}/Rooms/{room.RoomId}/Events.json?auth={UserClass.IdToken}";
		lisEvent.StartListen(EventUrl, OnRoomData, OnError);
	}

	private async void OnLeaveRoomPressed()
	{
		if (room == null) return;

		LeaveRoom.Disabled = true;
		GD.Print($"Đang xóa phòng: {room.RoomId}");

		try
		{
			var deleteSuccess = await room.DeleteAsync();

			if (deleteSuccess.Item1)
			{
				GD.Print("Xóa phòng thành công!");
				
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

	public async void LoadAvatar(int Seat, string Uid)
	{
		DisplayAvatar[Seat].Texture = await CloudinaryService.GetImageAsync(Uid);
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
}
