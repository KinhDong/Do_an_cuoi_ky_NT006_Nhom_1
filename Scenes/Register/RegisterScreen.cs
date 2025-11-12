using Godot;
using System;

public partial class RegisterScreen : Control
{
	private AnimationPlayer anim;
	private Button RegisterButton;
	private Button BackButton;

	public override void _Ready()
	{
		anim = GetNode<AnimationPlayer>("pn_Screen/AnimationPlayer");

		RegisterButton = GetNode<Button>("pn_Screen/pn_Register/btn_Register");
		RegisterButton.Pressed += OnRegisterButtonPressed;

		BackButton = GetNode<Button>("pn_Screen/pn_Register/btn_Back");
		BackButton.Pressed += OnBackButtonPressed;

		anim.Play("Register_Appear");
	}

	private void OnRegisterButtonPressed()
	{
		
	}
	
	private void OnBackButtonPressed()
	{
		GetTree().ChangeSceneToFile(@"Scenes\Login\LoginScreen.tscn");
	}
}
