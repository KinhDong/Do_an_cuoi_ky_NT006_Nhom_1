using Godot;
using System;
using NT106.Scripts.Models;
public partial class CreateOrJoinRoom : Node2D
{
	Button CreateRoom;
	Button JoinRoom;
	LineEdit RoomId;

	Button Return;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Nút "Tạo phòng"
		CreateRoom = GetNode<Button>("CreateOrJoin/btnCreateRoom");
		CreateRoom.Pressed += OpenCreateRoom;

		//Nút "Tham gia phòng"
		JoinRoom = GetNode<Button>("CreateOrJoin/btnJoinRoom");
		JoinRoom.Pressed += OnJoinRoomPressed;

		RoomId = GetNode<LineEdit>("CreateOrJoin/txtRoomID");

		//Nút "Quay về"
		Return = GetNode<Button>("CreateOrJoin/btnReturn");
		Return.Pressed += GoBackToGameMode;
	}

	//Bấm để quay về "Chọn chế độ chơi"
	private void GoBackToGameMode()
	{
		GetTree().ChangeSceneToFile("res://Scenes/GameModeSelection/ModeChoosing.tscn");
	}
	
	private void OpenCreateRoom()
    {
		GetTree().ChangeSceneToFile("res://Scenes/CreateRoom/CreateRoom.tscn");
    }

	private async void OnJoinRoomPressed()
    {
		try
        {
            string roomId = RoomId.Text.Trim();
			if(roomId == null) throw new Exception ("Vui lòng nhập mã phòng");

			var res = await RoomClass.JoinAsync(roomId);
			if(!res.Item1) throw new Exception(res.Item2);

			// Thành công
			GetTree().ChangeSceneToFile(@"Scenes\PlayAsPlayer\PlayAsPlayerScreen.tscn");
        }

		catch (Exception ex)
        {
            GD.PrintErr("Lỗi", ex.Message);
        }
		
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
