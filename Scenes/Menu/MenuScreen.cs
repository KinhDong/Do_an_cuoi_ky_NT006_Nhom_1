using Godot;
using NT106.Scripts.Models;
using System;

public partial class MenuScreen : Control
{
	Button ExitGameButton;

	public override void _Ready()
	{
		ExitGameButton = GetNode<Button>("pn_Background/pn_Menu/btn_ExitGame");
		ExitGameButton.Pressed += OnExitGameButtonPressed;
	}

	private async void OnExitGameButtonPressed()
	{
		// Đăng xuất cái đã
		await UserClass.LogoutAsync();

		GetTree().Quit();
	}
}
