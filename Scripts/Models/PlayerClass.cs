using Godot;
using NT106.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NT106.Scripts.Models
{
	public class PlayerClass
	{
		public string Uid { get; set; }        
		public string InGameName { get; set; }
		public long Money { get; set; }
		public Texture2D Avatar { get; set; }
		public int Seat { get; set; }
		public string JoinedAt { get; set; }
		public Dictionary<int, (int, int)> Hands {get; set;}
		
		public async Task LoadAvatarAsync()
		{
			Avatar = await CloudinaryService.GetImageAsync(Uid);
		}

		public (int, int) CaclulateScore()
		{
			int Score = 0;
			int Strength = 1;
			int aceCount = 0;

			for (int i = 0; i < Hands.Count; i++)
			{
				if (Hands[i].Item1 == 1)
				{
					aceCount++;
					Score += 11;
				}
				else if (Hands[i].Item1 > 10) Score += 10; // J, Q, K
				else Score += Hands[i].Item1;
			}            

			if(Hands.Count == 2)
			{
				if(aceCount == 2) return (21, 4);

				if(aceCount == 1)                
					if(Hands[0].Item1 >= 10 || Hands[1].Item1 >= 10)                    
						Strength = 3; // Xì dách
					   
				return (Score, Strength);
			}

			while (Score > 21 && aceCount > 0) // Giảm điểm nếu có A
			{
				Score -= 10;
				aceCount--;
			}

			if(Score > 21) return (Score, 0); // Quắc

			if (Hands.Count == 5) return (Score, 2); // Ngũ linh

			return (Score, Strength);
		}
	}
}
