using Godot;
using NT106.Scripts.Models;
using System;

public partial class ProfileScreen : Control
{
	[Export] private TextureRect Avatar;
	[Export] private TextureButton ChangeAvatar;
	[Export] private LineEdit UsernameLE;
	[Export] private LineEdit InGameNameLE;
	[Export] private TextureButton ChangeInGameName;
	[Export] private LineEdit Money;
	[Export] private Button Exit;
	[Export] private AnimationPlayer anim;
    [Export] public AudioStream BackgroundMusic;

	// Thêm một FileDialog để chọn ảnh
	private FileDialog avatarDialog;

	public override void _Ready()
	{
        // Phát nhạc nền
        if (BackgroundMusic != null)
        {
            AudioManager.Instance.PlayMusic(BackgroundMusic);
        }

        // Tải Avatar từ UserClass
        if (UserClass.Avatar != null)
		{
			Avatar.Texture = UserClass.Avatar;
		}

		ChangeAvatar.ToggleMode = false; 
		ChangeAvatar.Pressed += OnChangeAvatarPressed;

		// Tải Username
		UsernameLE.Text = UserClass.UserName;
		UsernameLE.Editable = false; 

		// Tải InGameName
		InGameNameLE.Text = UserClass.InGameName;
		InGameNameLE.Editable = false; 

		ChangeInGameName.Pressed += OnChangeInGameNamePressed;

		// Lấy node Money và gán giá trị
		Money.Text = UserClass.Money.ToString();
		Money.Editable = false; 

		Exit.Pressed += OnExitButtonPressed;

		anim.Play("Profile_Appear");

		// Khởi tạo FileDialog
		SetupAvatarDialog();
	}

	private void SetupAvatarDialog()
	{
		avatarDialog = new FileDialog();
		avatarDialog.Title = "Chọn ảnh đại diện";
		avatarDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
		avatarDialog.Access = FileDialog.AccessEnum.Filesystem;
		// Thêm bộ lọc cho file ảnh
		avatarDialog.AddFilter("*.png ; PNG Images");
		avatarDialog.AddFilter("*.jpg ; JPG Images");
		
		// Kết nối signal khi chọn file thành công
		avatarDialog.FileSelected += OnAvatarFileSelected;
		// Ẩn dialog khi nhấn Cancel
		avatarDialog.Canceled += () => avatarDialog.Hide();
		
		AddChild(avatarDialog);
	}

	private void OnChangeAvatarPressed()
	{
		// mở FileDialog
		avatarDialog.PopupCentered();
	}

	private void OnAvatarFileSelected(string path)
	{
		try
		{
			// Tải ảnh từ file để xem trước (preview) ngay lập tức
			var image = Image.LoadFromFile(path);
			if (image == null)
			{
				throw new Exception("Không thể tải file ảnh đã chọn.");
			}

			var texture = ImageTexture.CreateFromImage(image);
			Avatar.Texture = texture;

			// Gọi hàm của UserClass để tải ảnh lên server
			UserClass.ChangeAvatar(path);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Lỗi khi chọn avatar: {ex.Message}");
		}
	}


	private async void OnChangeInGameNamePressed()
	{
		
		if (ChangeInGameName.ButtonPressed) 
		{
			// Trạng thái LÀ true (hiện icon Edit) -> có nghĩa là user vừa nhấn "Lưu"
			
			InGameNameLE.Editable = false; // Khóa chỉnh sửa
			string newName = InGameNameLE.Text.Trim(); // Lấy tên mới và cắt khoảng trắng

			// Chỉ gọi API nếu tên có nội dung và khác với tên cũ
			if (!string.IsNullOrEmpty(newName) && newName != UserClass.InGameName)
			{
				try
				{
					// Gọi hàm đổi tên của UserClass
					await UserClass.ChangeInGameName(newName);
					// Cập nhật lại Text (phòng trường hợp server có thay đổi gì đó)
					InGameNameLE.Text = UserClass.InGameName; 
				}
				catch (Exception ex)
				{
					GD.PrintErr($"Lỗi đổi tên: {ex.Message}");
					// Nếu lỗi, trả lại tên cũ
					InGameNameLE.Text = UserClass.InGameName;
					// (Nên có một Popup báo lỗi cho người dùng)
				}
			}
			else
			{
				// Nếu tên rỗng hoặc không đổi, trả lại tên cũ
				InGameNameLE.Text = UserClass.InGameName;
			}
		}
		else 
		{
			// Trạng thái LÀ false (hiện icon Save) -> có nghĩa là user vừa nhấn "Sửa"
			
			InGameNameLE.Editable = true; // Cho phép sửa
			InGameNameLE.GrabFocus(); // Tự động focus vào ô
			InGameNameLE.SelectAll(); // Bôi đen toàn bộ text
		}
	}

	private void OnExitButtonPressed()
	{
		QueueFree();
	}
}
