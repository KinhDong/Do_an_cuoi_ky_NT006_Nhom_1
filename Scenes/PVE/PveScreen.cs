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
	Sprite2D[,] DisplayCards; 
	[Export] public AudioStream BackgroundMusic;

	private HashSet<(int Rank, int Suit)> DeckOfCards; 
	private bool IsGameActive = false;
	Random ran = new Random();
	#endregion

	#region --- SETUP & START ---
	public override void _Ready()
	{
		// Phát nhạc nền
        if (BackgroundMusic != null)
        {
            AudioManager.Instance.PlayMusic(BackgroundMusic);
        }

		ExitButton.Pressed += OnExitPressed;
		HitButton.Pressed += OnHitPressed;
		StandButton.Pressed += OnStandPressed;
		
		HitButton.Disabled = true;
		StandButton.Disabled = true;

		E = new PlayerClass 
		{ 
			InGameName = "Máy (Dealer)", 
			Avatar = ResourceLoader.Load<Texture2D>(@"Assets\Environment_Avatar.png"),
			Hands = new(),
			Money = 999999 
		};
		
		P = new PlayerClass 
		{ 
			InGameName = UserClass.InGameName ?? "Bạn", 
			Avatar = UserClass.Avatar,
			Hands = new(),
			Money = UserClass.Money 
		};

		DisplayE.Display(E);
		DisplayP.Display(P);

		DeckOfCards = new();
		// Tạo bộ bài
		for (int rank = 1; rank <= 13; rank++)
		{
			for (int suit = 1; suit <= 4; suit++)
			{
				DeckOfCards.Add((rank, suit));
			}
		}

		// Thiết lập hiển thị lá bài
		DisplayCards = new Sprite2D[2, 5];
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				DisplayCards[i, j] = GetNode<Sprite2D>($"pn_Background/ttr_Table/CardsOfPlayer{i}/Card{j}");
			}
		}

		// Tự động bắt đầu sau 5 giây
		GetTree().CreateTimer(5.0f).Timeout += () => StartGame();
	}

	private async void StartGame()
	{
		// Reset trạng thái
		IsGameActive = true;
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
		AddCardToHand(false, DrawCard());
		
		// Kiểm tra Xì Dách / Xì Bàn ngay lập tức (2 lá)
		if (CheckSpecialWin2Cards()) return;

		HitButton.Disabled = false;
		StandButton.Disabled = false;
	}

	private void OnHitPressed()
	{
		if (!IsGameActive) return;

		AddCardToHand(true, DrawCard());

		var (score, strength) = P.CaclulateScore();
		
		// 1. Kiểm tra Quắc
		if (score > 21)
		{
			EndGame(false, $"Bạn được {score} điểm -> Quắc (Bust)!");
		}
		// 2. Kiểm tra Ngũ Linh 
		else if (strength == 2)
		{
			OS.Alert("Đã đủ 5 lá (Ngũ Linh). Tự động Dằn!");
			OnStandPressed(); 
		}
	}

	private async void OnStandPressed()
	{
		if (!IsGameActive) return;

		// --- KIỂM TRA ĐỦ TUỔI (16+) ---
		var (pScore, pStrength) = P.CaclulateScore();
		if (pScore < 16)
		{
			OS.Alert($"Bạn mới có {pScore} điểm. Phải đủ 16 điểm mới được Dằn (Non)!");
			return;
		}

		HitButton.Disabled = true;
		StandButton.Disabled = true;

		// Máy rút bài: Buộc phải rút nếu < 17 (Luật cái)
		while (E.CaclulateScore().Item1 < 17 && E.Hands.Count < 5)
		{
			await Task.Delay(300);
			AddCardToHand(false, DrawCard());
		}

		await Task.Delay(500);
		CompareResult();
	}

	private void CompareResult()
	{
		var (pScore, pStrength) = P.CaclulateScore();
		var (eScore, eStrength) = E.CaclulateScore();

		if (pStrength == 2 && eStrength != 2) 
		{ 
			EndGame(true, "Bạn được Ngũ Linh! Bạn thắng."); 
			return; 
		}
		if (eStrength == 2 && pStrength != 2) 
		{ 
			EndGame(false, "Máy được Ngũ Linh! Bạn thua."); 
			return; 
		}
		
		// Máy Quắc
		if (eStrength == 0)
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
		var pStrength = P.CaclulateScore().Item2;
		var eStrength = E.CaclulateScore().Item2;

		if (pStrength < 2 && eStrength < 2) return false;

		if (pStrength > eStrength)
		{
			if (pStrength == 2)
				EndGame(true, "Bạn có Xì Dách! Bạn thắng ngay.");
			else 
				EndGame(true, "Bạn có Xì Bàn! Bạn thắng ngay.");
			return true;
		}

		if (eStrength > pStrength)
		{
			if (eStrength == 2)
				EndGame(false, "Máy có Xì Dách! Bạn thua ngay.");
			else 
				EndGame(false, "Máy có Xì Bàn! Bạn thua ngay.");
			return true;
		}

		EndGame(false, "Cả hai cùng có bài đẹp. Nhà cái thắng.");
		return true;		
	}

	#endregion

	#region --- UTILS & NETWORK ---

	private async void EndGame(bool isWin, string message)
	{
		ShowCards(0, E.Hands);
		await Task.Delay(3000);
		IsGameActive = false;
		HitButton.Disabled = true;
		StandButton.Disabled = true;

		if (isWin)
		{
			P.Money += 5;
			OS.Alert($"CHIẾN THẮNG!\n{message}\n(+5 xu)");
			UserClass.Money += 5;
			DisplayP.UpdateMoney(UserClass.Money);

			try 
			{ 
				await FirebaseApi.Put($"Users/{UserClass.Uid}/Money", UserClass.Money);
				DisplayP.UpdateMoney(UserClass.Money); 
			}
			catch (Exception ex) 
			{ 
				GD.PrintErr(ex.Message); 
			}
		}
		else
		{
			OS.Alert($"THẤT BẠI!\n{message}\n");
		}

		// Ẩn bài và thu bài về bộ
		UnShowCards(0);
		UnShowCards(1);
		foreach (var card in P.Hands)
			DeckOfCards.Add(card);
		foreach (var card in E.Hands)
			DeckOfCards.Add(card);
		P.Hands.Clear();
		E.Hands.Clear();
				
		await Task.Delay(2000);
		StartGame();
	}

	private (int Rank, int Suit) DrawCard()
	{
		var card = DeckOfCards.ElementAt(ran.Next(DeckOfCards.Count)); // Bốc 1 lá
			DeckOfCards.Remove(card);	
		return card;
	}

	private async void AddCardToHand(bool isPlayer, (int Rank, int Suit) card)
	{
		if (!isPlayer)
		{
			E.Hands.Add(card);
			anim.Play("DealPlayer0");
			await Task.Delay(300);
			ShowCard(0, 0, E.Hands[0]);
		}
		else
		{
			int cardIndex = P.Hands.Count;
			P.Hands.Add(card);
			anim.Play($"DealCard{cardIndex}");
			await Task.Delay(300);
			ShowCard(1, cardIndex, card);
		}

		anim.Queue("RESET");
	}

	private void ShowCard(int seat, int cardIndex, (int, int) card) // Hiển thị 1 lá bài
	{
		DisplayCards[seat, cardIndex].Frame = (card.Item1 - 1) + (card.Item2 - 1) * 13;
		DisplayCards[seat, cardIndex].Visible = true;
	}

	private void ShowCards(int seat, List<(int Rank, int Suit)> hands) // Hiển thị các lá bài của 1 player
	{
		for(int i = 0; i < hands.Count; i++)
			ShowCard(seat, i, hands[i]);
	}

	private void UnShowCards(int seat)
	{
		for(int i = 0; i < 5; i++)        
			DisplayCards[seat, i].Visible = false;
		DisplayCards[seat, 0].Frame = 52; // Mặt sau lá bài
	}
	
	private void OnExitPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/GameModeSelection/ModeChoosing.tscn");
	}

	#endregion
}
