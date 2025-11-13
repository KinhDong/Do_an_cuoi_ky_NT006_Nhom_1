using Godot;
using System;
using NT106.Scripts.Models;

public partial class ModeChoosing : Node2D
{
	Button PvP;
	Button PvE;
	Button Return;
	public override void _Ready()
	{
		//Nút "Chơi với người"
		PvP = GetNode<Button>("ModeBackground/btnPVP");
		PvP.Pressed += OpenCreateOrJoin;

		//Nút chơi với máy
		PvE = GetNode<Button>("ModeBackground/btnPVE");

		//Nút "Quay về"
		Return = GetNode<Button>("ModeBackground/btnReturn");
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
