using Godot;
using NT106.Scripts.Models;
using System;

public partial class MenuScreen : Control
{
	private Button PlayButton;
	private Button HowToPlayButton;
	private Button ExitGameButton;
	private Button SettingButton;
	private Button ProfileButton;

	public override void _Ready()
	{
		PlayButton = GetNode<Button>("pn_Background/btn_Play");

		HowToPlayButton = GetNode<Button>("pn_Background/btn_HowToPlay");

		ExitGameButton = GetNode<Button>("pn_Background/btn_ExitGame");
		ExitGameButton.Pressed += OnExitGameButtonPressed;

		SettingButton = GetNode<Button>("pn_Background/btn_Setting");

		ProfileButton = GetNode<Button>("pn_Background/btn_Profile");
		ProfileButton.Pressed += OnProfileButtonPressed;
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
