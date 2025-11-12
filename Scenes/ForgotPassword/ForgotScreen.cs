using Godot;
using NT106.Scripts.Models;
using System;

public partial class ForgotScreen : Node2D
{
	private AnimationPlayer anim;
	private LineEdit UsernameLineEdit;
	private Button ResetPasswordButton;
	private Button BackButton;

	public override void _Ready()
	{
		anim = GetNode<AnimationPlayer>("AnimationPlayer");

		UsernameLineEdit = GetNode<LineEdit>("pn_Screen/pn_ForgotPassword/le_Username");

		ResetPasswordButton = GetNode<Button>("pn_Screen/pn_ForgotPassword/btn_ResetPassword");
		ResetPasswordButton.Pressed += OnResetPasswordButtonPressed;

		BackButton = GetNode<Button>("pn_Screen/pn_ForgotPassword/btn_Back");
		BackButton.Pressed += OnBackButtonPressed;

		anim.Play("ForgotPassword_Appear");
	}

	private async void OnResetPasswordButtonPressed()
	{
		string username = UsernameLineEdit.Text.Trim();

		string email = await UserClass.GetEmailFromUsernameAsync(username);

		await UserClass.SendPasswordResetEmailAsync(email);
	}

	private void OnBackButtonPressed()
	{
		GetTree().ChangeSceneToFile(@"Scenes\Login\LoginScreen.tscn");
	}
}
