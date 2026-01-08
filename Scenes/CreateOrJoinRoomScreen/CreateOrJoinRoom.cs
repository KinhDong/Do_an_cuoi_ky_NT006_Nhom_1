using Godot;
using System;
using NT106.Scripts.Models;
public partial class CreateOrJoinRoom : Node2D
{
	[Export] Button CreateRoom;
	[Export] Button JoinRoom;
	[Export] LineEdit RoomId;

	[Export] Button Return;
    [Export] public AudioStream BackgroundMusic;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        // Phát nhạc nền
        if (BackgroundMusic != null)
        {
            AudioManager.Instance.PlayMusic(BackgroundMusic);
        }

        //Nút "Tạo phòng"
        CreateRoom.Pressed += OpenCreateRoom;

		//Nút "Tham gia phòng"
		JoinRoom.Pressed += OnJoinRoomPressed;

		//Nút "Quay về"
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
			GetTree().ChangeSceneToFile("res://Scenes/PlayAsPlayer/PlayAsPlayerScreen.tscn");
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
