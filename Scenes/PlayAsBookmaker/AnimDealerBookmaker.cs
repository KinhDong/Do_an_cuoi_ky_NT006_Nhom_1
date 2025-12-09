using Godot;
using System;
using System.Collections.Generic;
using NT106.Scripts.Models;
using System.Threading.Tasks;
using Godot.Collections;

public partial class AnimDealerBookmaker
{
	[Export] private AnimationPlayer anim;

	[Export] Array<Sprite2D> PlayerCards; // Các lá bài của người chơi

	public void DealCardToPlayer(string pid, (int, int) card)
	{
		int seat = RoomClass.CurrentRoom.Players[pid].Seat;
		anim.Play($"DealPlayer{seat}");
		Task.Delay(300); // Chờ animation chạy đến vị trí người chơi
		anim.Queue("RESET");
	}

	public void DealCardToYou((int, int) card)
	{
		int cardIndex = RoomClass.CurrentRoom.Players[UserClass.Uid].Hands.Count;
		anim.Play($"DealCard{cardIndex}");
		Task.Delay(300);
		PlayerCards[cardIndex].Frame = (card.Item1 - 1) + (card.Item2 - 1) * 13;
		anim.Queue("RESET");
	}

	public void ShowAllCards(string pid)
	{
		
		for (int cardIndex = 0; cardIndex < RoomClass.CurrentRoom.Players[pid].Hands.Count; cardIndex++)
		{
			var card = RoomClass.CurrentRoom.Players[pid].Hands[cardIndex];
		}
	}
	

	public void ResetDisplay()
	{
		for(int playerIndex = 0; playerIndex < 4; playerIndex++)
		{
			for(int cardIndex = 0; cardIndex < 5; cardIndex++)
			{
				
			}

		}
	}
}
