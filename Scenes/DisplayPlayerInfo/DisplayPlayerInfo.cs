using Godot;
using NT106.Scripts.Models;
using System;

public partial class DisplayPlayerInfo : Control
{
	[Export] private Node2D Group;
	[Export] private TextureRect AvatarDisplay;
	[Export] private LineEdit NameDisplay;
	[Export] private LineEdit MoneyDisplay;

	public void Display(PlayerClass player)
	{
		AvatarDisplay.Texture = player.Avatar;
		NameDisplay.Text = player.InGameName;
		MoneyDisplay.Text = player.Money.ToString();

		Group.Visible = true;
	}	

	public void UpdateMoney(long money)
    {
        MoneyDisplay.Text = money.ToString();
    }
}
