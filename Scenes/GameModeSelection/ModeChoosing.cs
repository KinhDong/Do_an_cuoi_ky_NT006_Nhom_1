using Godot;
using System;
using NT106.Scripts.Models;

public partial class ModeChoosing : Node2D
{
	[Export] Button PvP;
	[Export] Button PvE;
	[Export] Button Return;

	public override void _Ready()
	{
		// Nút "Chơi với người"
		PvP.Pressed += OpenCreateOrJoin;

		// Nút "Chơi với máy" -> ĐÃ THÊM CODE Ở ĐÂY
		if (PvE != null)
		{
			PvE.Pressed += OpenPvE;
		}
		else
		{
			GD.PrintErr("Lỗi: Chưa gán nút PvE trong Inspector!");
		}

		// Nút "Quay về"
		Return.Pressed += GoBackToMenu;
	}

	// Bấm "Chơi với người" để tới "Tạo phòng"
	private void OpenCreateOrJoin()
	{
		GetTree().ChangeSceneToFile("res://Scenes/CreateOrJoinRoomScreen/CreateOrJoinRoom.tscn");
	}

	//  Bấm "Chơi với máy" để tới màn hình PvE
	private void OpenPvE()
	{
		// Đường dẫn tới Scene PvE mà bạn đã tạo
		GetTree().ChangeSceneToFile("res://Scenes/PVE/PveScreen.tscn");
	}
	
	// Bấm "Quay về" để trở về Menu
	private void GoBackToMenu()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Menu/MenuScreen.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
