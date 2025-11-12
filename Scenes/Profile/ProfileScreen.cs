using Godot;
using System;

public partial class ProfileScreen : Control
{
    Button ChangAvatarButton;
    Button BackButton;

    public override void _Ready()
    {
        ChangAvatarButton = GetNode<Button>("pn_Background/btn_ChangeAvatar");

        BackButton = GetNode<Button>("pn_Background/btn_Back");
        BackButton.Pressed += OnBackButtonPressed;
    }

    private void OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile(@"Scenes\Menu\MenuScreen.tscn");
    }
}
