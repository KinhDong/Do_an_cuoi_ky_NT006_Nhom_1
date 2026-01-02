using Godot;
using System;
using NT106.Scripts.Models;

public partial class LoginScreen : Control
{
	[Export] private AnimationPlayer anim;
	[Export] private LineEdit UsernameLineEdit;
	[Export] private LineEdit PasswordLineEdit;
	[Export] private Button LoginButton;
	[Export] private Button RegisterButton;
	[Export] private LinkButton ForgotPasswordButton;
	[Export] private Label ErrorLabel;
	private Texture2D EyeOpenTexture;
	private Texture2D EyeClosedTexture;
	[Export] private TextureButton HidePasswordButton;
    [Export] public AudioStream BackgroundMusic;

    // Mở màn hình
    public override void _Ready()
	{
        // Phát nhạc nền
        if (BackgroundMusic != null)
        {
            AudioManager.Instance.PlayMusic(BackgroundMusic);
        }

        LoginButton.Pressed += OnLogginButtonPressed;

		RegisterButton.Pressed += OnRegisterButtonPressed;

		ForgotPasswordButton.Pressed += OnForgotPasswordButtonPressed;

		EyeOpenTexture = ResourceLoader.Load<Texture2D>(@"Assets\OpenEye_Icon.png");
		EyeClosedTexture = ResourceLoader.Load<Texture2D>(@"Assets\CloseEye_Icon.png");

		HidePasswordButton.TextureNormal = EyeClosedTexture;
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
		if (PasswordLineEdit.Secret)
		{
			HidePasswordButton.TextureNormal = EyeOpenTexture;
			PasswordLineEdit.Secret = false;
		}

		else
		{
			HidePasswordButton.TextureNormal = EyeClosedTexture;
			PasswordLineEdit.Secret = true;
		}
	}
}
