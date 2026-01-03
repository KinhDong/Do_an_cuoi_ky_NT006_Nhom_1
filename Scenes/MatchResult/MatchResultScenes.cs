using Godot;
using NT106.Scripts.Models;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class MatchResultScenes : Control
{
	[Export] Label RoomIdLabel;
	[Export] Label TimeLabel;
	[Export] GridContainer ResultContainer;

	SingleResultDisplay DisplayYourResult;
	SingleResultDisplay DisplayOpponentResult;

	public override void _Ready()
	{
		DisplayYourResult = new SingleResultDisplay();
		DisplayOpponentResult = new SingleResultDisplay();

		DisplayYourResult.GetNodes(ResultContainer, 1);
		DisplayOpponentResult.GetNodes(ResultContainer, 2);
	}

	public void Display(MatchHistory matchHistory)
	{
		RoomIdLabel.Text = matchHistory.RoomId;
		TimeLabel.Text = matchHistory.Datetime;
		DisplayYourResult.Display(matchHistory.You);
		DisplayOpponentResult.Display(matchHistory.Opponent);
	}
}

partial class SingleResultDisplay : Node2D
{
	public Label PlayerNameLabel;
	public Label RoleLabel;
	public Label ScoreLabel;
	public Label StrengthLabel;
	public Label MoneyChangedLabel;

	public void GetNodes(GridContainer container, int index)
	{
		PlayerNameLabel = container.GetChild<Label>(index * 5 + 0);
		RoleLabel = container.GetChild<Label>(index * 5 + 1);
		ScoreLabel = container.GetChild<Label>(index * 5 + 2);
		StrengthLabel = container.GetChild<Label>(index * 5 + 3);
		MoneyChangedLabel = container.GetChild<Label>(index * 5 + 4);
	}

	public void Display(PlayerResult playerResult)
	{
		PlayerNameLabel.Text = playerResult.PlayerInGameName;
		RoleLabel.Text = playerResult.Role;
		ScoreLabel.Text = playerResult.Score.ToString();
		
		// Hiển thị Strength dưới dạng chuỗi
		switch (playerResult.Strength)
		{
			case 0:
				StrengthLabel.Text = "Quắc";
				break;
			case 1:
				StrengthLabel.Text = "";
				break;
			case 2:
				StrengthLabel.Text = "Ngũ linh";
				break;
			case 3:
				StrengthLabel.Text = "Xì dách";
				break;
			case 4:
				StrengthLabel.Text = "Xì bàn";
				break;

			default:
				StrengthLabel.Text = "Không xác định";
				break;
		}
		MoneyChangedLabel.Text = playerResult.Result.ToLower() == "win" ? $"+{playerResult.MoneyChange}" : 
		playerResult.Result.ToLower() == "lose" ? $"-{playerResult.MoneyChange}" : "0";
	}
}
