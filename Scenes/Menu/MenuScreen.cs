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
	[Export] Button MatchHistoryButton;

    [Export] public AudioStream BackgroundMusic;
	public override void _Ready()
	{
        // Phát nhạc nền
        if (BackgroundMusic != null)
        {
            AudioManager.Instance.PlayMusic(BackgroundMusic);
        }
        PlayButton.Pressed += OpenModeSeclectionScreen;

		HowToPlayButton.Pressed += OnHowToPlayButtonPressed;

		ExitGameButton.Pressed += OnExitGameButtonPressed;

		ProfileButton.Pressed += OnProfileButtonPressed;

		LeaderboardButton.Pressed += OnLeaderboardButtonPressed;

		MatchHistoryButton.Pressed += OnMatchHistoryButtonPressed;
	}

	//Mở màn hình chọn chế độ chơi
	private void OpenModeSeclectionScreen()
	{
		GetTree().ChangeSceneToFile("res://Scenes/GameModeSelection/ModeChoosing.tscn");
	}

	private void OnHowToPlayButtonPressed()
	{
		var HowToPlayScene = GD.Load<PackedScene>(@"Scenes\HowToPlayScenes\HowToPlayScenes.tscn");

		AddChild(HowToPlayScene.Instantiate());
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

	private void OnMatchHistoryButtonPressed()
	{
		var MatchHistoryScene = GD.Load<PackedScene>(@"Scenes\MatchHistories\MatchHistoriesScenes.tscn");

		AddChild(MatchHistoryScene.Instantiate());
	}
}
