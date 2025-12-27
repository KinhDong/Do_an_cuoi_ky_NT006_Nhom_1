using Godot;
using Godot.Collections;
using NT106.Scripts.Services;
using System;
using System.Linq;
using NT106.Scripts.Models;

public partial class Leaderboard : Node2D
{
	[Export] TextureButton CloseButton;
	[Export] Array<Label> InGameNames;
	[Export] Array<Label> Moneys;
	[Export] Label YourRankLabel;
	System.Collections.Generic.Dictionary <string, UserData> usersData = new(); 

	public override void _Ready()
	{
		CloseButton.Pressed += () => { QueueFree(); };
		
		LoadLeaderboard();
	}

	public async void LoadLeaderboard()
	{
		try
		{
			usersData = await FirebaseApi.Get<
			System.Collections.Generic.Dictionary <string, UserData>>("Users");

			var sortedUsersData = usersData.OrderByDescending(u => u.Value.Money).ToList();
			GD.Print("Leaderboard data loaded successfully.");

			for (int i = 0; i < 10; i++)
			{
				if (i < sortedUsersData.Count)
				{
					InGameNames[i].Text = sortedUsersData[i].Value.InGameName;
					Moneys[i].Text = sortedUsersData[i].Value.Money.ToString();
				}
				else
				{
					InGameNames[i].Text = "-";
					Moneys[i].Text = "-";
				}
			}

			// Tìm vị trí của người chơi hiện tại
			int playerRank = sortedUsersData.FindIndex(u => u.Key == UserClass.Uid) + 1;
			if (playerRank > 0)
			{
				YourRankLabel.Text = playerRank.ToString();
				InGameNames[10].Text = UserClass.InGameName;
				Moneys[10].Text = usersData[UserClass.Uid].Money.ToString();
			}
			else
			{
				YourRankLabel.Text = "Unranked";
			}
		}

		catch (Exception e)
		{
			GD.PrintErr("Error loading leaderboard: " + e.Message);
		}
	}
}

public class UserData
{
	public string InGameName;
	public int Money;

	public UserData(string inGameName, int money)
	{
		InGameName = inGameName;
		Money = money;
	}
}
