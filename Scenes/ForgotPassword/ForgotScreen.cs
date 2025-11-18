using Godot;
using NT106.Scripts.Models;
using System;
using System.Threading.Tasks; 

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
		// Vô hiệu hóa nút để tránh spam click
		ResetPasswordButton.Disabled = true;

		try
		{
			string username = UsernameLineEdit.Text.Trim();

			// Kiểm tra username rỗng
			if (string.IsNullOrEmpty(username))
			{
				// Tương đương với MessageBox.Show()
				OS.Alert("Vui lòng nhập tên tài khoản", "Thông báo");
				return; // Dừng thực thi
			}

			// Lấy email từ username
			string email = await UserClass.GetEmailFromUsernameAsync(username);

			// Kiểm tra tài khoản có tồn tại không
			if (string.IsNullOrEmpty(email))
			{
				OS.Alert("Không tìm thấy tài khoản", "Thông báo");
				return; // Dừng thực thi
			}

			// Gửi email reset
			bool result = await UserClass.SendPasswordResetEmailAsync(email);

			// Thông báo kết quả
			if (result)
			{
				OS.Alert("Đã gửi email đặt lại mật khẩu. Vui lòng kiểm tra email của bạn.", "Thành công");
				// Có thể thêm lệnh chuyển về màn hình đăng nhập nếu muốn
				// OnBackButtonPressed(); 
			}
			else
			{
				// Thêm trường hợp xử lý lỗi nếu SendPasswordResetEmailAsync trả về false
				OS.Alert("Không thể gửi email đặt lại mật khẩu. Vui lòng thử lại.", "Lỗi");
			}
		}
		catch (Exception ex)
		{
			// Bắt lỗi (mất kết nối, lỗi API...)
			GD.PrintErr("Lỗi khi gửi email reset: ", ex.Message); // In ra console của Godot
			OS.Alert("Đã xảy ra lỗi: " + ex.Message, "Lỗi");
		}
		finally
		{
			// Luôn luôn kích hoạt lại nút, dù thành công hay thất bại
			// Kiểm tra IsInstanceValid để đảm bảo node vẫn tồn tại sau khi await
			if (IsInstanceValid(ResetPasswordButton))
			{
				ResetPasswordButton.Disabled = false;
			}
		}
	}

	private void OnBackButtonPressed()
	{
		GetTree().ChangeSceneToFile(@"Scenes\Login\LoginScreen.tscn");
	}
}
