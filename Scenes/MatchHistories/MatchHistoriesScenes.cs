using Godot;
using NT106.Scripts.Models;
using NT106.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class MatchHistoriesScenes : Node2D
{
	[Export] Button ExitButton;
	[Export] VBoxContainer MatchHistoryContainer;
	Dictionary<string, MatchHistory> MatchHistories;

	public override void _Ready()
	{
		ExitButton.Pressed += OnExitButtonPressed;

		// Lấy dữ liệu và hiển thị
		_ = LoadAndDisplayHistories();
	}

	private async Task LoadAndDisplayHistories()
	{
		await GetMatchHistories();

		// Hiển thị dữ liệu từ cuối đến đầu
		if (MatchHistories != null && MatchHistories.Count > 0)
		{
			MatchHistories = MatchHistories.Reverse().ToDictionary(item => item.Key, item => item.Value);
			foreach (var matchHistory in MatchHistories.Values)
			{
				var matchHistoryResult = GD.Load<PackedScene>(@"Scenes\MatchResult\MatchResultScenes.tscn").Instantiate<MatchResultScenes>();
				MatchHistoryContainer.AddChild(matchHistoryResult);
				matchHistoryResult.Display(matchHistory);

				GD.Print(matchHistory.RoomId);
			}
		}
		else
		{
			GD.Print("No match histories to display.");
		}
	}

	private async Task GetMatchHistories()
	{
		GD.Print("Fetching match histories...");
		
		MatchHistories = new();
		MatchHistories = await FirebaseApi.Get<Dictionary<string, MatchHistory>>(
			$"Users/{UserClass.Uid}/MatchHistories");

		if (MatchHistories == null || MatchHistories.Count == 0)
		{
			GD.Print("No match histories found.");
		}
	}

	private void OnExitButtonPressed()
	{
		QueueFree();
	}
}
