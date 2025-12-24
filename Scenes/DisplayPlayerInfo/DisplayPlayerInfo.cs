using Godot;
using NT106.Scripts.Models;
using System;
using System.Threading.Tasks;

public partial class DisplayPlayerInfo : Control
{
	[Export] private Node2D Group;
	[Export] private TextureRect AvatarDisplay;
	[Export] private LineEdit NameDisplay;
	[Export] private LineEdit MoneyDisplay;
	[Export] private Label TimerDisplay;
	[Export] private Timer timer;
	int timeLeft;

	public override void _Ready()
	{
		timer.Timeout += OnTimeout;
	}

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

	public async void StartCountdown() // Đếm ngược
	{
		TimerDisplay.Visible = true;

		timeLeft = 10;
		TimerDisplay.Text = timeLeft.ToString();
		timer.Start();
	}

	public void EndCountdown()
	{
		timer.Stop();
		TimerDisplay.Visible = false;
	}

	public void OnTimeout()
	{
		timeLeft--;
		TimerDisplay.Text = timeLeft.ToString();
		if(timeLeft < -20)
			EndCountdown();
	}
}
