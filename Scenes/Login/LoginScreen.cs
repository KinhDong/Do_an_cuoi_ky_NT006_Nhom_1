using Godot;
using System;
using NT106.Scripts.Models;

public partial class LoginScreen : Control
{
	private AnimationPlayer anim;
	private LineEdit UsernameLineEdit;
	private LineEdit PasswordLineEdit;
	private Button LoginButton;
	private Button RegisterButton;
	private LinkButton ForgotPasswordButton;
	private Label ErrorLabel;
	private TextureButton HidePasswordButton;

	// Mở màn hình
	public override void _Ready()
	{
		anim = GetNode<AnimationPlayer>("AnimationPlayer");

		UsernameLineEdit = GetNode<LineEdit>("pn_Screen/pn_Login/le_Username");

		PasswordLineEdit = GetNode<LineEdit>("pn_Screen/pn_Login/le_Password");

		LoginButton = GetNode<Button>("pn_Screen/pn_Login/btn_Login");
		LoginButton.Pressed += OnLogginButtonPressed;

		RegisterButton = GetNode<Button>("pn_Screen/pn_Login/btn_Register");
		RegisterButton.Pressed += OnRegisterButtonPressed;

		ForgotPasswordButton = GetNode<LinkButton>("pn_Screen/pn_Login/lbtn_ForgotPassword");
		ForgotPasswordButton.Pressed += OnForgotPasswordButtonPressed;

		ErrorLabel = GetNode<Label>("pn_Screen/pn_Login/lb_Error");

		HidePasswordButton = GetNode<TextureButton>("pn_Screen/pn_Login/ttbtn_HidePassword");
		HidePasswordButton.Pressed += OnHidePasswordButtonPressed;

		anim.Play("Login_Appear");
	}
	
	// Nhấn nút Đăng nhập
	private async void OnLogginButtonPressed()
	{
		string username = UsernameLineEdit.Text.Trim();
		string password = PasswordLineEdit.Text.Trim();

		if(username == String.Empty || password == String.Empty)
		{
			ErrorLabel.Text = "Vui lòng nhập đầy đủ thông tin";

			return;
		}

		var result = await UserClass.LoginAsync(username, password);

		if (result.Item1)
		{
			// Chuyển sang màn hình Menu
			GetTree().ChangeSceneToFile(@"Scenes\Menu\MenuScreen.tscn");
		}
		
		else
		{
			// Hiện label báo lỗi tương ứng
			ErrorLabel.Text = result.Item2;
		}
	}

	// Nhấn nút Đăng kí
	private void OnRegisterButtonPressed()
	{
		// Chuyển sang màn hình Đăng kí
		GetTree().ChangeSceneToFile(@"Scenes\Register\RegisterScreen.tscn");
	}

	// Nhấn nút quên mật khẩu
	private void OnForgotPasswordButtonPressed()
	{
		GetTree().ChangeSceneToFile(@"Scenes\ForgotPassword\ForgotScreen.tscn");
	}

	// Chuyển đổi trạng thái ẩn mật khẩu
	private void OnHidePasswordButtonPressed()
	{
		if (HidePasswordButton.ButtonPressed)
		{
			PasswordLineEdit.Secret = true;
		}

		else
		{
			PasswordLineEdit.Secret = false;
		}
	}
}
