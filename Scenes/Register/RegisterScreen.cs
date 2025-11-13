using Godot;
using System;
using System.Text.RegularExpressions; 
using NT106.Scripts.Models; 

public partial class RegisterScreen : Control
{
	private AnimationPlayer anim;
	private Button RegisterButton;
	private Button BackButton;
	
	private LineEdit usernameInput;
	private LineEdit emailInput;
	private LineEdit passwordInput;
	private LineEdit confirmInput;

	public override void _Ready()
	{
		anim = GetNode<AnimationPlayer>("pn_Screen/AnimationPlayer");

		RegisterButton = GetNode<Button>("pn_Screen/pn_Register/btn_Register");
		RegisterButton.Pressed += OnRegisterButtonPressed;

		BackButton = GetNode<Button>("pn_Screen/pn_Register/btn_Back");
		BackButton.Pressed += OnBackButtonPressed;

		usernameInput = GetNode<LineEdit>("pn_Screen/pn_Register/le_Username");

		emailInput = GetNode<LineEdit>("pn_Screen/pn_Register/le_Password2"); 

		passwordInput = GetNode<LineEdit>("pn_Screen/pn_Register/le_Password");
		
		confirmInput = GetNode<LineEdit>("pn_Screen/pn_Register/le_ConfirmPassword");

		anim.Play("Register_Appear");
	}

	/// Được gọi khi nhấn nút Đăng ký.
	private async void OnRegisterButtonPressed()
	{
		// Lấy text từ các ô LineEdit
		string username = usernameInput.Text.Trim();
		string email = emailInput.Text.Trim();
		string password = passwordInput.Text.Trim();
		string confirm = confirmInput.Text.Trim();

		if(username == String.Empty || email == String.Empty ||
		password == String.Empty || confirm == String.Empty)
        {
            OS.Alert("Vui lòng nhập đầy đủ thông tin", "Lỗi");
			return;
        }

		// KIỂM TRA ĐỊNH DẠNG EMAIL 
		if (!IsValidEmail(email))
		{
			OS.Alert("Định dạng email không hợp lệ! Vui lòng kiểm tra lại.", "Lỗi");
			return; // Dừng lại, không gọi UserClass
		}
		// KẾT THÚC KIỂM TRA 

		// Tắt nút đăng ký để tránh nhấn nhiều lần
		RegisterButton.Disabled = true;

		// Gọi hàm RegisterAsync từ UserClass
		var (success, message) = await UserClass.RegisterAsync(username, email, password, confirm);

		// Mở lại nút đăng ký sau khi có kết quả
		RegisterButton.Disabled = false;

		// Xử lý kết quả
		if (success)
		{
			// Đăng ký thành công
			OS.Alert("Đăng ký thành công!");

			GetTree().ChangeSceneToFile(@"Scenes\Login\LoginScreen.tscn");
		}
		else
		{
			// Đăng ký thất bại
			OS.Alert($"Lỗi đăng ký: {message}", "Lỗi");
		}
	}
	
	/// Được gọi khi nhấn nút Quay lại.
	private void OnBackButtonPressed()
	{
		// Quay lại màn hình Đăng nhập
		GetTree().ChangeSceneToFile(@"Scenes\Login\LoginScreen.tscn");
	}

	// HÀM KIỂM TRA EMAIL
	/// Kiểm tra một chuỗi có phải là định dạng email hợp lệ hay không.
	private bool IsValidEmail(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
			return false;

		try
		{
			// Sử dụng Regex để kiểm tra định dạng email cơ bản
			return Regex.IsMatch(email, 
				@"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
				RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
		}
		catch (RegexMatchTimeoutException)
		{
			// Trường hợp hiếm gặp Regex mất quá nhiều thời gian
			return false;
		}
	}
}
