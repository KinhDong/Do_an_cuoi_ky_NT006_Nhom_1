using Godot;
using NT106.Scripts.Models;

using System;

public partial class ProfileScreen : Control
{
	private TextureRect Avatar;
	private TextureButton ChangeAvatar;
	private LineEdit UsernameLE;
	private LineEdit InGameNameLE;
	private TextureButton ChangeInGameName;
	private LineEdit Money;
	private Button Exit;
	private AnimationPlayer anim;

	public override void _Ready()
	{
		Avatar = GetNode<TextureRect>("Panel/pn_Profile/pn_Outline/ttr_Avatar");
		// Avatar.Texture = UserClass.Avatar

		ChangeAvatar = GetNode<TextureButton>("Panel/pn_Profile/ttbtn_ChangeAvatar");
		ChangeAvatar.Pressed += OnChangeAvatarPressed;

		UsernameLE = GetNode<LineEdit>("Panel/pn_Profile/le_Username");
		// UsernameLE.Text = UserClass.Username

		InGameNameLE = GetNode<LineEdit>("Panel/pn_Profile/le_InGameName");
		//

		ChangeInGameName = GetNode<TextureButton>("Panel/pn_Profile/ttbtn_ChangeInGameName");
		ChangeInGameName.Pressed += OnChangeInGameNamePressed;

		// Money...

		Exit = GetNode<Button>("Panel/pn_Profile/btn_Exit");
		Exit.Pressed += OnExitButtonPressed;

		anim = GetNode<AnimationPlayer>("AnimationPlayer");
		anim.Play("Profile_Appear");
	}

	private void OnChangeAvatarPressed()
	{
		if(ChangeAvatar.ButtonPressed) // Trạng thái Edit
		{
			// Mở khung Dialog chọn ảnh...
		}

		else // Trạng thái Save
		{
			// Gọi thay đổi ảnh trong UserClass...
		}
	}

	private void OnChangeInGameNamePressed()
	{
		if(ChangeInGameName.ButtonPressed)
		{
			InGameNameLE.Editable = false; // Cho phép sửa
		}

		else
		{
			InGameNameLE.Editable = true;

			// Gọi hàm đổi tên của UserClass ...
		}
	}

	private void OnExitButtonPressed()
	{
		QueueFree();
	}
}
