using Godot;
using System;

public partial class HowToPlayScenes : Node2D
{
	[Export] Button BackButton;

	public override void _Ready()
	{
		BackButton.Pressed += OnBackButtonPressed;
	}

	private void OnBackButtonPressed()
	{
		GetTree().ChangeSceneToFile(@"Scenes\Menu\MenuScreen.tscn");
	}
}
