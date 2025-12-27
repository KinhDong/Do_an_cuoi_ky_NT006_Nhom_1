using Godot;
using NT106.Scripts.Models;
using System;

public partial class MenuScreen : Control
{
	[Export] Button PlayButton;
	[Export] Button HowToPlayButton;
	[Export] Button ExitGameButton;
	[Export] Button SettingButton;
	[Export] Button ProfileButton;
	[Export] TextureButton LeaderboardButton;

	public override void _Ready()
	{
		PlayButton.Pressed += OpenModeSeclectionScreen;

		ExitGameButton.Pressed += OnExitGameButtonPressed;

		ProfileButton.Pressed += OnProfileButtonPressed;

		LeaderboardButton.Pressed += OnLeaderboardButtonPressed;
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

	private void OnLeaderboardButtonPressed()
	{
		var LeaderboardScene = GD.Load<PackedScene>(@"Scenes\Leaderboard\Leaderboard.tscn");

		AddChild(LeaderboardScene.Instantiate());
	}
}
