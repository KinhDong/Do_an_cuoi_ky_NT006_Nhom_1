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
		//Nút "Chơi với người"
		PvP.Pressed += OpenCreateOrJoin;

		//Nút chơi với máy

		//Nút "Quay về"
		Return.Pressed += GoBackToMenu;
	}

	//Bấm "Chơi với người" để tới "Tạo phòng"
	private void OpenCreateOrJoin()
	{
		GetTree().ChangeSceneToFile("res://Scenes/CreateOrJoinRoomScreen/CreateOrJoinRoom.tscn");
	}
	
	//Bấm "Quay về" để trở về Menu
	private void GoBackToMenu()
    {
		GetTree().ChangeSceneToFile("res://Scenes/Menu/MenuScreen.tscn");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
