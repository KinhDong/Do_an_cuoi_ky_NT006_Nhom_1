using Godot;
using NT106.Scripts.Models;
using System;

public partial class MenuScreen : Control
{
	Button PlayButton;
	Button HowToPlayButton;
	Button ExitGameButton;
	Button SettingButton;
	TextureButton ProfileButton;

	public override void _Ready()
	{
		PlayButton = GetNode<Button>("pn_Background/btn_Play");
		PlayButton.Pressed += OpenModeSeclectionScreen;

		HowToPlayButton = GetNode<Button>("pn_Background/btn_HowToPlay");

		ExitGameButton = GetNode<Button>("pn_Background/btn_ExitGame");
		ExitGameButton.Pressed += OnExitGameButtonPressed;

		SettingButton = GetNode<Button>("pn_Background/btn_Setting");

		ProfileButton = GetNode<TextureButton>("pn_Background/tttbtn_Profile");
		// ProfileButton.TextureNormal = UserClass.Avatar;
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
}
