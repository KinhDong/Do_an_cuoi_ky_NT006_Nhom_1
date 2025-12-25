using Godot;
using Godot.Collections;
using NT106.Scripts.Models;
using NT106.Scripts.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class PveScreen : Node2D
{
	#region --- KHAI BÁO BIẾN ---
	[Export] Button ExitButton;
	[Export] Button HitButton;
	[Export] Button StandButton;

	PlayerClass E; // Máy
	PlayerClass P; // Người

	[Export] DisplayPlayerInfo DisplayE, DisplayP;
	[Export] AnimationPlayer anim;    
	[Export] Array<Sprite2D> ECards; 
	[Export] Array<Sprite2D> PCards; 

	private List<(int Rank, int Suit)> DeckOfCards; 
	private const long BET_AMOUNT = 500; 
	private bool IsGameActive = false;

	private List<(int Rank, int Suit)> HandPlayer = new();
	private List<(int Rank, int Suit)> HandEnemy = new();
	#endregion

	#region --- SETUP & START ---
	public override void _Ready()
	{
		ExitButton.Pressed += OnExitPressed;
		HitButton.Pressed += OnHitPressed;
		StandButton.Pressed += OnStandPressed;
		
		HitButton.Disabled = true;
		StandButton.Disabled = true;

		E = new PlayerClass 
		{ 
			InGameName = "Máy (Dealer)", 
			Money = 999999 
		};
		
		P = new PlayerClass 
		{ 
			InGameName = UserClass.InGameName ?? "Bạn", 
			Money = UserClass.Money 
		};

		DisplayE.Display(E);
		DisplayP.Display(P);
		ResetCardSprites();

		// Tự động bắt đầu sau 1 giây
		GetTree().CreateTimer(1.0f).Timeout += () => StartGame();
	}

	private async void StartGame()
	{
		// Cứu trợ nếu hết tiền
		if (P.Money < BET_AMOUNT)
		{
			long relief = 1000;
			P.Money += relief;
			UserClass.Money = P.Money;
			await UpdateUserMoney(UserClass.Uid, P.Money);
			OS.Alert($"Nhà cái hỗ trợ bạn {relief} xu để chơi tiếp!");
		}

		// Trừ tiền cược
		P.Money -= BET_AMOUNT;
		UserClass.Money = P.Money;
		DisplayP.Display(P);
		await UpdateUserMoney(UserClass.Uid, P.Money);

		// Reset trạng thái
		IsGameActive = true;
		HandPlayer.Clear();
		HandEnemy.Clear();
		ResetCardSprites();
		InitializeDeck();

		await DealInitialCards();
	}
	#endregion

	#region --- LOGIC GAME CHÍNH ---

	private async Task DealInitialCards()
	{
		await Task.Delay(300); 
		AddCardToHand(true, DrawCard());

		await Task.Delay(300); 
		AddCardToHand(false, DrawCard());

		await Task.Delay(300); 
		AddCardToHand(true, DrawCard());
		
		await Task.Delay(300);
		var hiddenCard = DrawCard();
		HandEnemy.Add(hiddenCard);
		DisplayCardOnTable(ECards[1], hiddenCard.Rank, hiddenCard.Suit, isFaceUp: false);
		
		// Kiểm tra Xì Dách / Xì Bàn ngay lập tức (2 lá)
		if (CheckSpecialWin2Cards()) return;

		HitButton.Disabled = false;
		StandButton.Disabled = false;
	}

	private void OnHitPressed()
	{
		if (!IsGameActive) return;

		AddCardToHand(true, DrawCard());

		int score = CalculateScore(HandPlayer);
		
		// 1. Kiểm tra Quắc
		if (score > 21)
		{
			EndGame(false, $"Bạn được {score} điểm -> Quắc (Bust)!");
		}
		// 2. Kiểm tra Ngũ Linh 
		else if (HandPlayer.Count == 5)
		{
			 OS.Alert("Đã đủ 5 lá (Ngũ Linh). Tự động Dằn!");
			 OnStandPressed(); 
		}
	}

	private async void OnStandPressed()
	{
		if (!IsGameActive) return;

		// --- KIỂM TRA ĐỦ TUỔI (16+) ---
		int pScore = CalculateScore(HandPlayer);
		if (pScore < 16 && HandPlayer.Count < 5)
		{
			OS.Alert($"Bạn mới có {pScore} điểm. Phải đủ 16 điểm mới được Dằn (Non)!");
			return;
		}

		HitButton.Disabled = true;
		StandButton.Disabled = true;

		// Lật bài máy
		var hiddenCard = HandEnemy[1];
		DisplayCardOnTable(ECards[1], hiddenCard.Rank, hiddenCard.Suit, true);

		// Máy rút bài: Buộc phải rút nếu < 17 (Luật cái)
		while (CalculateScore(HandEnemy) < 17)
		{
			await Task.Delay(800);
			if (HandEnemy.Count < 5)
			{
				AddCardToHand(false, DrawCard());
			}
			else 
			{
				break; // Đủ 5 lá
			}
		}

		await Task.Delay(500);
		CompareResult();
	}

	private void CompareResult()
	{
		int pScore = CalculateScore(HandPlayer);
		int eScore = CalculateScore(HandEnemy);

		bool p5 = (HandPlayer.Count == 5 && pScore <= 21);
		bool e5 = (HandEnemy.Count == 5 && eScore <= 21);

		if (p5 && !e5) 
		{ 
			EndGame(true, "Bạn được Ngũ Linh! Bạn thắng."); 
			return; 
		}
		if (!p5 && e5) 
		{ 
			EndGame(false, "Máy được Ngũ Linh! Bạn thua."); 
			return; 
		}
		
		// Máy Quắc
		if (eScore > 21)
		{
			EndGame(true, $"Máy được {eScore} điểm (Quắc). Bạn thắng!");
			return;
		}

		if (pScore > eScore) EndGame(true, $"Bạn {pScore} - Máy {eScore}. Bạn Thắng!");
		else if (pScore < eScore) EndGame(false, $"Bạn {pScore} - Máy {eScore}. Bạn Thua!");
		else EndGame(false, $"Hòa ({pScore} điểm). Nhà cái thắng hòa.");
	}

	private bool CheckSpecialWin2Cards()
	{
		int pScore = CalculateScore(HandPlayer); 
		bool pXiBan = IsXiBan(HandPlayer);
		bool pXiDach = IsXiDach(HandPlayer);

		bool eXiBan = IsXiBan(HandEnemy);
		bool eXiDach = IsXiDach(HandEnemy);

		if (pXiBan || pXiDach || eXiBan || eXiDach)
		{
			var hidden = HandEnemy[1];
			DisplayCardOnTable(ECards[1], hidden.Rank, hidden.Suit, true);

			if ((pXiBan || pXiDach) && !(eXiBan || eXiDach)) 
				EndGame(true, "Bạn có Xì Dách/Xì Bàn! Bạn thắng ngay.");
			else if (!(pXiBan || pXiDach) && (eXiBan || eXiDach)) 
				EndGame(false, "Máy có Xì Dách/Xì Bàn! Bạn thua.");
			else 
				EndGame(false, "Cả hai cùng có bài đẹp. Nhà cái thắng.");
			
			return true;
		}
		return false;
	}

	#endregion

	#region --- TÍNH ĐIỂM ---

	private int CalculateScore(List<(int Rank, int Suit)> hand)
	{
		int score = 0;
		int aceCount = 0;

		foreach (var card in hand)
		{
			int val = card.Rank + 1; 
			if (val >= 10) score += 10; 
			else if (val == 1) 
			{
				aceCount++;
				score += 11; 
			}
			else score += val;
		}

		while (score > 21 && aceCount > 0)
		{
			score -= 10;
			aceCount--;
		}
		return score;
	}

	private bool IsXiBan(List<(int Rank, int Suit)> hand) 
	{
		return hand.Count == 2 && (hand[0].Rank == 0 && hand[1].Rank == 0);
	}

	private bool IsXiDach(List<(int Rank, int Suit)> hand)
	{
		if (hand.Count != 2) return false;
		bool hasAce = (hand[0].Rank == 0 || hand[1].Rank == 0);
		bool hasTen = (hand[0].Rank >= 9 || hand[1].Rank >= 9); 
		return hasAce && hasTen;
	}

	#endregion

	#region --- UTILS & NETWORK ---

	private async void EndGame(bool isWin, string message)
	{
		IsGameActive = false;
		HitButton.Disabled = true;
		StandButton.Disabled = true;

		if (isWin)
		{
			P.Money += (BET_AMOUNT * 2);
			OS.Alert($"CHIẾN THẮNG!\n{message}\n(+{BET_AMOUNT} xu)");
		}
		else
		{
			OS.Alert($"THẤT BẠI!\n{message}\n(-{BET_AMOUNT} xu)");
		}

		UserClass.Money = P.Money;
		DisplayP.Display(P);
		
		await UpdateUserMoney(UserClass.Uid, P.Money);
		await Task.Delay(2000);
		StartGame();
	}

	private async Task UpdateUserMoney(string uid, long newMoney)
	{
		try 
		{ 
			await FirebaseApi.Patch($"Users/{uid}.json?auth={UserClass.IdToken}", JsonConvert.SerializeObject(new { Money = newMoney })); 
		}
		catch (Exception ex) 
		{ 
			GD.PrintErr(ex.Message); 
		}
	}

	private void InitializeDeck()
	{
		DeckOfCards = new List<(int, int)>();
		for (int s = 0; s < 4; s++) 
		{
			for (int r = 0; r < 13; r++) 
			{
				DeckOfCards.Add((r, s));
			}
		}

		// Fisher-Yates Shuffle
		Random rng = new Random();
		int n = DeckOfCards.Count;
		while (n > 1) 
		{ 
			n--; 
			int k = rng.Next(n + 1); 
			(DeckOfCards[k], DeckOfCards[n]) = (DeckOfCards[n], DeckOfCards[k]); 
		}
	}

	private (int Rank, int Suit) DrawCard()
	{
		if (DeckOfCards.Count == 0) InitializeDeck();
		var c = DeckOfCards[0]; 
		DeckOfCards.RemoveAt(0); 
		return c;
	}

	private void AddCardToHand(bool isPlayer, (int Rank, int Suit) card)
	{
		var targetList = isPlayer ? HandPlayer : HandEnemy;
		var targetSprites = isPlayer ? PCards : ECards;
		
		targetList.Add(card);
		int idx = targetList.Count - 1;

		if (idx < targetSprites.Count)
		{
			DisplayCardOnTable(targetSprites[idx], card.Rank, card.Suit, true);
			
			if (isPlayer && idx <= 4) 
				anim.Play($"DealCard{idx}");
			else 
				targetSprites[idx].Visible = true;
		}
	}

	private void DisplayCardOnTable(Sprite2D s, int r, int suit, bool isFaceUp)
	{
		s.Visible = true;
		s.FrameCoords = isFaceUp ? new Vector2I(r, suit) : new Vector2I(0, 4);
	}

	private void ResetCardSprites()
	{
		foreach (var c in PCards) c.Visible = false;
		foreach (var c in ECards) c.Visible = false;
	}
	
	private void OnExitPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/GameModeSelection/ModeChoosing.tscn");
	}

	#endregion
}
