using Godot;
using System;
using NT106.Scripts.Models;

public partial class PlayAsBookmakerScreen : Node2D
{
	// Thông tin phòng
	private RoomClass room {set; get;}

	// Hiển thị mã phòng và mức cược
	private LineEdit DisplayRoomId;
	private LineEdit DisplayBetAmount;

	// Hiển thị Avatar
	private TextureRect DisplayBookmakerAvatar;
	private TextureRect DisplayPlayer1Avatar, DisplayPlayer2Avatar;
	private TextureRect DisplayPlayer3Avatar, DisplayPlayer4Avatar, DisplayPlayer5Avatar;

	// Hiển thị tên
	private LineEdit DisplayBookmakerName;
	private LineEdit DisplayPlayer1Name, DisplayPlayer2Name;
	private LineEdit DisplayPlayer3Name, DisplayPlayer4Name, DisplayPlayer5Name;

	// Hiển thị số tiền    
	private LineEdit DisplayBookmakerMoney;
	private LineEdit DisplayPlayer1Money, DisplayPlayer2Money;
	private LineEdit DisplayPlayer3Money, DisplayPlayer4Money, DisplayPlayer5Money;

	// Rời phòng
	private Button LeaveRoom;


	public override void _Ready()
	{
		room = RoomClass.CurrentRoom;

		DisplayRoomId = GetNode<LineEdit>("pn_Background/le_RoomId");
		DisplayRoomId.Text = room.RoomId;
		DisplayBetAmount = GetNode<LineEdit>("pn_Background/ttr_Table/le_BetAmount");
		DisplayBetAmount.Text = room.BetAmount.ToString();

		DisplayBookmakerAvatar = GetNode<TextureRect>("pn_Background/ttr_Avatar_Bookmaker");
		DisplayBookmakerName = GetNode<LineEdit>("pn_Background/ttr_Avatar_Bookmaker/le_InGameName_You");
		DisplayBookmakerMoney = GetNode<LineEdit>("pn_Background/ttr_Avatar_Bookmaker/le_Money");

		DisplayPlayer1Avatar = GetNode<TextureRect>("pn_Background/ttr_Avatar_Player1");
		DisplayPlayer1Name = GetNode<LineEdit>("pn_Background/ttr_Avatar_Player1/le_InGameName_You");
		DisplayPlayer1Money = GetNode<LineEdit>("pn_Background/ttr_Avatar_Player1/le_Money");

		if(room.MaxPlayers > 2)
		{            
			DisplayPlayer2Avatar = GetNode<TextureRect>("pn_Background/ttr_Avatar_Player2");            
			DisplayPlayer2Name = GetNode<LineEdit>("pn_Background/ttr_Avatar_Player2/le_InGameName_You");
			DisplayPlayer2Money = GetNode<LineEdit>("pn_Background/ttr_Avatar_Player2/le_Money");
			DisplayPlayer2Avatar.Visible = true; // Hiển thị
		}


		LeaveRoom = GetNode<Button>("pn_Background/btn_LeaveRoom");
		LeaveRoom.Pressed += OnLeaveRoomPressed;
	}

	private async void OnLeaveRoomPressed()
	{
		if (room == null) return;

		LeaveRoom.Disabled = true;
		GD.Print($"Đang xóa phòng: {room.RoomId}");

		try
		{
			bool deleteSuccess = await room.DeleteAsync();

			if (deleteSuccess)
			{
				GD.Print("Xóa phòng thành công!");
				
				// XÓA KHỎI STATIC VARIABLE
				RoomClass.CurrentRoom = null;
				GetTree().ChangeSceneToFile("res://Scenes/CreateRoom/CreateRoom.tscn");
			}
			else
			{
				GD.PrintErr("❌ Không thể xóa phòng!");
				LeaveRoom.Disabled = false;
			}
		}
		
		catch (Exception ex)
		{
			GD.PrintErr($"❌ Lỗi khi xóa phòng: {ex.Message}");
			LeaveRoom.Disabled = false;
		}
	}
}
