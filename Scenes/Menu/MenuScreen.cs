using Godot;
using NT106.Scripts.Models;
using System;

public partial class MenuScreen : Control
{
	Button PlayButton;
	Button HowToPlayButton;
	Button ExitGameButton;
	Button SettingButton;
	Button ProfileButton;

	public override void _Ready()
	{
		PlayButton = GetNode<Button>("pn_Background/btn_Play");
		PlayButton.Pressed += OpenModeSeclectionScreen;

		HowToPlayButton = GetNode<Button>("pn_Background/btn_HowToPlay");

		ExitGameButton = GetNode<Button>("pn_Background/btn_ExitGame");
		ExitGameButton.Pressed += OnExitGameButtonPressed;

		SettingButton = GetNode<Button>("pn_Background/btn_Setting");

		ProfileButton = GetNode<Button>("pn_Background/btn_Profile");
		ProfileButton.Pressed += OnProfileButtonPressed;
	}

	//Mở màn hình chọn chế độ chơi
	private void OpenModeSeclectionScreen()
	{
		GetTree().ChangeSceneToFile("res://Scenes/GameModeSelection/ModeChoosing.tscn");
	}

	private async void OnExitGameButtonPressed()
	{
		// Đăng xuất cái đã
		await UserClass.LogoutAsync();

		GetTree().Quit();
	}

	private void OnProfileButtonPressed()
	{
		var ProfileScene = GD.Load<PackedScene>(@"Scenes/Profile/ProfileScreen.tscn");

		AddChild(ProfileScene.Instantiate());
	}
}
