using Godot;
using System;
using NT106.Scripts.Models;
using System.Collections.Generic;
using CloudinaryDotNet;
using NT106.Scripts.Services;

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


	public async override void _Ready()
	{
		room = RoomClass.CurrentRoom;

		DisplayAvatar = new() {null, null, null, null};
		DisplayName = new() {null, null, null, null};
		DisplayMoney = new() {null, null, null, null};		

		// Hiển thị thông tin chung
		DisplayRoomId = GetNode<LineEdit>("pn_Background/le_RoomId");
		DisplayRoomId.Text = room.RoomId;
		DisplayBetAmount = GetNode<LineEdit>("pn_Background/ttr_Table/le_BetAmount");
		DisplayBetAmount.Text = room.BetAmount.ToString();

		// Gán thông tin PLayer
		GetNodesForPlayers();

		// Hiển thị 
		UidDisplayed[0] = room.HostId;
		room.Players[room.HostId].Avatar = await CloudinaryService.GetImageAsync(room.HostId); // Lấy Avatar trên Cloudinary
		room.Players[room.HostId].Seat = 0;
		Display(0, room.Players[room.HostId]);

		UidDisplayed[1] = UserClass.Uid;
		Display(1, room.Players[UserClass.Uid]);

		int SeatIndex = 2;
		foreach(var p in room.Players)
		{
			if(p.Key != UserClass.Uid)
			{
				UidDisplayed[SeatIndex] = p.Key;
				room.Players[p.Key].Seat = SeatIndex;
				room.Players[p.Key].Avatar = await CloudinaryService.GetImageAsync(p.Key);
				Display(SeatIndex, room.Players[p.Key]);

				SeatIndex++;
			}
		}

		while(SeatIndex < 4) AvilableSlot.Add(SeatIndex); // Lưu các vị trí trống

		// Rời phòng
		LeaveRoom = GetNode<Button>("pn_Background/btn_LeaveRoom");
		LeaveRoom.Pressed += OnLeaveRoomPressed;
	}

	private async void OnLeaveRoomPressed()
	{
		try
		{
			var res = await room.LeaveAsync();

			if(!res.Item1) throw new Exception(res.Item2);

			GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.cs");	
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
		DisplayAvatar[Seat].Texture = player.Avatar;
		DisplayName[Seat].Text = player.InGameName;
		DisplayMoney[Seat].Text = player.Money.ToString();
	}
}
