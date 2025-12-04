using Godot;
using System;
using NT106.Scripts.Models;
using System.Collections.Generic;
using NT106.Scripts.Services;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

public partial class PlayAsBookmakerScreen : Node2D
{
	#region --- VARIABLES & NODES ---

	// Data
	public RoomClass currentRoom { get; set; }
	public HashSet<CardClass> DeckOfCards;
	public HashSet<int> AvailableSlots { get; set; } = new HashSet<int> { 1, 2, 3 };
	private Dictionary<string, List<CardClass>> HandCache = new Dictionary<string, List<CardClass>>();
	private List<string> TurnOrder;
	private int CurrentTurnIndex = 0;

	// UI Nodes
	public List<TextureRect> AvatarList { get; set; } = new() { null, null, null, null };
	public List<LineEdit> NameList { get; set; } = new() { null, null, null, null };
	public List<LineEdit> MoneyList { get; set; } = new() { null, null, null, null };
	public List<string> UidDisplayed { get; set; } = new() { null, null, null, null }; // Map: SeatIndex -> UserID

	private Button btnStartGame;
	private Button btnLeaveRoom;
	private Button btnHit;
	private Button btnStand;
	private LineEdit txtRoomId;
	private LineEdit txtBetAmount;

	// Network
	private FirebaseStreaming EventListener;
	private const float CARD_SPACING = 35.0f;

	#endregion

	#region --- INITIALIZATION ---

	public override void _Ready()
	{
		GD.Print($"[HOST START] ID: {UserClass.Uid}");

		if (RoomClass.CurrentRoom == null)
		{
			GetTree().ChangeSceneToFile(@"Scenes\CreateRoom\CreateRoom.tscn");
			return;
		}

		currentRoom = RoomClass.CurrentRoom;

		SetupUIReferences();
		SetupHostInfo();
		SetupButtons();
		SetupNetwork();
	}

	private void SetupUIReferences()
	{
		// Map UI Nodes
		for (int i = 0; i < 4; i++)
		{
			AvatarList[i] = GetNode<TextureRect>($"pn_Background/ttr_Avatar_Player{i}");
			NameList[i] = GetNode<LineEdit>($"pn_Background/ttr_Avatar_Player{i}/le_InGameName_You");
			MoneyList[i] = GetNode<LineEdit>($"pn_Background/ttr_Avatar_Player{i}/le_Money");
		}

		txtRoomId = GetNode<LineEdit>("pn_Background/le_RoomId");
		txtBetAmount = GetNode<LineEdit>("pn_Background/ttr_Table/le_BetAmount");
		
		// Set Initial Values
		txtRoomId.Text = currentRoom.RoomId;
		txtBetAmount.Text = currentRoom.BetAmount.ToString();
	}

	private void SetupHostInfo()
	{
		// Host luôn ngồi ghế 0
		UidDisplayed[0] = currentRoom.HostId;
		AvatarList[0].Texture = UserClass.Avatar;
		DisplayPlayerInfo(0, currentRoom.Players[currentRoom.HostId]);
	}

	private void SetupButtons()
	{
		btnLeaveRoom = GetNode<Button>("pn_Background/btn_LeaveRoom");
		btnStartGame = GetNode<Button>("pn_Background/btn_StartGame");
		btnHit = GetNode<Button>("pn_Background/btn_Hit");
		btnStand = GetNode<Button>("pn_Background/btn_Stand");

		// Events
		btnLeaveRoom.Pressed += OnLeaveRoomPressed;
		
		if (currentRoom.HostId == UserClass.Uid)
		{
			btnStartGame.Visible = true;
			btnStartGame.Pressed += OnStartGame;
		}
		else
		{
			btnStartGame.Visible = false;
		}

		// Action Buttons
		if (btnHit != null) btnHit.Pressed += () => OnPlayerRequestAction("hit");
		if (btnStand != null) btnStand.Pressed += () => OnPlayerRequestAction("stand");

		btnHit.Visible = false;
		btnStand.Visible = false;
	}

	private void SetupNetwork()
	{
		EventListener = new(FirebaseApi.BaseUrl, $"Rooms/{currentRoom.RoomId}/Events", UserClass.IdToken);
		EventListener.OnData += (json) =>
		{
			try
			{
				var evt = json.ToObject<NT106.Scripts.Models.MessageEvent>();
				if (evt != null && evt.data != null)
				{
					_ = ProcessEvent(evt.data.user, evt.data.type, evt.data);
				}
			}
			catch (Exception e)
			{
				GD.PrintErr($"[HOST EVENT ERROR] {e.Message}");
			}
		};
		EventListener.Start();
	}

	#endregion

	#region --- EVENT PROCESSING ---

	private Task ProcessEvent(string pid, string type, dynamic data = null)
	{
		switch (type)
		{
			case "join":
				CallDeferred(nameof(HandleJoin), pid);
				break;
			case "leave":
				CallDeferred(nameof(HandleLeave), pid);
				break;
			case "request_action":
				string action = data.action;
				GD.Print($"[HOST EVENT] Action '{action}' từ {pid}");
				CallDeferred(nameof(HandlePlayerAction), pid, action);
				break;
			case "deal":
				var cardData = data.card;
				string r = cardData.Rank; string s = cardData.Suit; int v = (int)cardData.Value;
				var newCard = new CardClass { Rank = r, Suit = s, Value = v };

				// Cache bài để lật cuối game
				if (!HandCache.ContainsKey(pid)) HandCache[pid] = new List<CardClass>();
				HandCache[pid].Add(newCard);

				// Host chỉ thấy bài mình
				bool isFaceUp = (pid == UserClass.Uid);
				CallDeferred(nameof(DisplayCardAnimation), pid, s, r, isFaceUp);
				break;
			case "end_game":
				CallDeferred(nameof(RevealAllCards));
				break;
		}
		return Task.CompletedTask;
	}

	#endregion

	#region --- GAME LOGIC (CORE) ---

	private async void HandlePlayerAction(string playerUid, string action)
	{
		GD.Print($"[HOST LOGIC] {playerUid} -> {action}");

		// 1. Kiểm tra lượt
		if (playerUid != currentRoom.CurrentTurn)
		{
			GD.PrintErr($"[LỖI] Sai lượt! Hiện tại là: {currentRoom.CurrentTurn}");
			return;
		}

		if (action == "hit")
		{
			await ProcessHitAction(playerUid);
		}
		else if (action == "stand")
		{
			GD.Print($"-> Dằn. Chuyển lượt.");
			NextTurn();
		}
	}

	private async Task ProcessHitAction(string playerUid)
	{
		try
		{
			// Kiểm tra bộ bài
			if (DeckOfCards == null || DeckOfCards.Count == 0) InitializeDeck();

			// Rút bài
			Random rand = new Random();
			int index = rand.Next(DeckOfCards.Count);
			var card = DeckOfCards.ElementAt(index);
			DeckOfCards.Remove(card);

			// Lưu dữ liệu an toàn
			if (currentRoom.Players.TryGetValue(playerUid, out var player))
			{
				if (player.Cards == null) player.Cards = new List<CardClass>();
				player.Cards.Add(card);
			}
			else
			{
				GD.PrintErr($"CRASH: Không tìm thấy Player {playerUid}");
				return;
			}

			// Gửi Event Deal
			var dealEvent = new NT106.Scripts.Models.RoomEvent 
			{ 
				type = "deal", 
				user = playerUid, 
				time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 
				card = card 
			};

			GD.Print($"[HOST] Chia {card.Rank} {card.Suit}");
			await FirebaseApi.Post($"Rooms/{currentRoom.RoomId}/Events.json?auth={UserClass.IdToken}", dealEvent);

			// Kiểm tra điểm
			int score = currentRoom.CalculateScore(currentRoom.Players[playerUid].Cards);
			GD.Print($"-> Điểm mới: {score}");

			if (score >= 21)
			{
				GD.Print("-> Đủ điểm/Quắc. Chuyển lượt.");
				await Task.Delay(1000);
				NextTurn();
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[CRASH HIT]: {ex.Message}\n{ex.StackTrace}");
		}
	}

	private async Task DealCardsLogic()
	{
		try
		{
			GD.Print("--- BẮT ĐẦU CHIA BÀI ---");
			TurnOrder = new List<string>();

			// Lấy danh sách khách trước
			var guests = currentRoom.Players.Values
				.Where(p => p != null && p.Uid != currentRoom.HostId)
				.OrderBy(p => p.Seat)
				.ToList();
			
			foreach (var g in guests) TurnOrder.Add(g.Uid);
			
			// Thêm Host vào cuối
			TurnOrder.Add(currentRoom.HostId);

			CurrentTurnIndex = -1;
			
			// Reset bài cũ
			foreach (var p in currentRoom.Players.Values) if (p != null) p.Cards = new List<CardClass>();

			// Chia 2 vòng
			Random rand = new Random();
			for (int i = 0; i < 2; i++)
			{
				foreach (var uid in TurnOrder)
				{
					if (DeckOfCards.Count == 0) InitializeDeck();
					
					int index = rand.Next(DeckOfCards.Count);
					var card = DeckOfCards.ElementAt(index);
					DeckOfCards.Remove(card);

					if (currentRoom.Players.ContainsKey(uid)) currentRoom.Players[uid].Cards.Add(card);

					var dealEvent = new NT106.Scripts.Models.RoomEvent 
					{ 
						type = "deal", 
						user = uid, 
						time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 
						card = card 
					};
					await FirebaseApi.Post($"Rooms/{currentRoom.RoomId}/Events.json?auth={UserClass.IdToken}", dealEvent);
					await Task.Delay(250);
				}
			}
			await Task.Delay(1000);
			NextTurn();
		}
		catch (Exception ex) { GD.PrintErr($"Err Deal: {ex.Message}"); }
	}

	private async void NextTurn()
	{
		try
		{
			CurrentTurnIndex++;
			
			// Nếu hết danh sách -> Kết thúc game
			if (CurrentTurnIndex >= TurnOrder.Count)
			{
				currentRoom.CurrentTurn = null;
				await CompareAndEndGame();
				return;
			}

			string nextPlayerUid = TurnOrder[CurrentTurnIndex];
			currentRoom.CurrentTurn = nextPlayerUid;
			GD.Print($"[HOST] Next Turn: {nextPlayerUid}");

			// Nếu là lượt Host -> Hiện nút
			if (nextPlayerUid == UserClass.Uid)
			{
				btnHit.Visible = true;
				btnStand.Visible = true;
				int score = currentRoom.CalculateScore(currentRoom.Players[UserClass.Uid].Cards);
				if (score >= 21)
				{
					OS.Alert("Nhà cái đủ điểm/Quắc, tự động dằn!");
					HandlePlayerAction(UserClass.Uid, "stand");
				}
			}
			else
			{
				btnHit.Visible = false;
				btnStand.Visible = false;
			}

			// Gửi Event chuyển lượt
			var turnEvent = new NT106.Scripts.Models.RoomEvent 
			{ 
				type = "change_turn", 
				user = nextPlayerUid, 
				time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() 
			};
			await FirebaseApi.Post($"Rooms/{currentRoom.RoomId}/Events.json?auth={UserClass.IdToken}", turnEvent);
		}
		catch (Exception ex) { GD.PrintErr($"Err NextTurn: {ex.Message}"); }
	}

	private async Task CompareAndEndGame()
	{
		btnHit.Visible = false;
		btnStand.Visible = false;

		var hostHand = currentRoom.Players[currentRoom.HostId].Cards;
		var (hostRank, hostScore) = currentRoom.GetHandStrength(hostHand);
		Dictionary<string, long> results = new Dictionary<string, long>();

		try
		{
			foreach (var player in currentRoom.Players.Values)
			{
				if (player.Uid == currentRoom.HostId) continue;

				var (playerRank, playerScore) = currentRoom.GetHandStrength(player.Cards);
				long bet = currentRoom.BetAmount;
				long change = 0;

				// Logic so bài
				if (playerRank > hostRank) change = bet;
				else if (playerRank < hostRank) change = -bet;
				else
				{
					if (hostRank == HandRank.Bust && playerRank == HandRank.Bust) change = 0;
					else
					{
						if (playerScore > hostScore) change = bet;
						else if (playerScore < hostScore) change = -bet;
						else change = 0;
					}
				}

				results.Add(player.Uid, change);
				if (change != 0)
				{
					currentRoom.Players[currentRoom.HostId].Money -= change;
					player.Money += change;
					await UpdateUserMoney(player.Uid, player.Money);
				}
			}

			await UpdateUserMoney(currentRoom.HostId, currentRoom.Players[currentRoom.HostId].Money);
			
			// Update UI
			foreach (var p in currentRoom.Players.Values)
			{
				await FirebaseApi.Patch($"Rooms/{currentRoom.RoomId}/Players/{p.Uid}.json?auth={UserClass.IdToken}", JsonConvert.SerializeObject(new { Money = p.Money }));
			}
			DisplayPlayerInfo(0, currentRoom.Players[currentRoom.HostId]);
			foreach (var p in currentRoom.Players.Values) if (p.Uid != currentRoom.HostId) DisplayPlayerInfo(p.Seat, p);

			// Send End Game Event
			var endEvent = new NT106.Scripts.Models.RoomEvent 
			{ 
				type = "end_game", 
				user = UserClass.Uid, 
				time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 
				results = results, 
				hostScore = hostScore 
			};
			await FirebaseApi.Post($"Rooms/{currentRoom.RoomId}/Events.json?auth={UserClass.IdToken}", endEvent);
			
			currentRoom.Status = RoomStatus.WAITING;
			await FirebaseApi.Patch($"Rooms/{currentRoom.RoomId}/Status.json?auth={UserClass.IdToken}", JsonConvert.SerializeObject(RoomStatus.WAITING));
			
			btnStartGame.Visible = true;
			OS.Alert($"KẾT QUẢ ĐÃ CÓ!");
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Err Result: {ex.Message}");
			btnStartGame.Visible = true;
		}
	}

	#endregion

	#region --- UI & HELPERS ---

	private void DisplayCardAnimation(string uid, string suit, string rank, bool isFaceUp)
	{
		int seatIndex = -1;
		for (int i = 0; i < 4; i++) { if (UidDisplayed[i] == uid) { seatIndex = i; break; } }
		if (seatIndex == -1) return;

		string atlasPath = "res://Assets/AllCards.png";
		var fullTexture = ResourceLoader.Load<Texture2D>(atlasPath);
		if (fullTexture == null) return;

		float w = fullTexture.GetWidth() / 13.0f;
		float h = fullTexture.GetHeight() / 5.0f;
		int x = 0, y = 0;

		if (isFaceUp)
		{
			switch (rank) { case "A": x = 0; break; case "2": x = 1; break; case "3": x = 2; break; case "4": x = 3; break; case "5": x = 4; break; case "6": x = 5; break; case "7": x = 6; break; case "8": x = 7; break; case "9": x = 8; break; case "10": x = 9; break; case "J": x = 10; break; case "Q": x = 11; break; case "K": x = 12; break; }
			switch (suit) { case "Spades": y = 0; break; case "Clubs": y = 1; break; case "Diamonds": y = 2; break; case "Hearts": y = 3; break; }
		}
		else
		{
			x = 0; y = 4; // Mặt sau
		}

		AtlasTexture at = new AtlasTexture();
		at.Atlas = fullTexture;
		at.Region = new Rect2(x * w, y * h, w, h);

		TextureRect tr = new TextureRect();
		tr.Texture = at;
		tr.CustomMinimumSize = new Vector2(60, 90);
		tr.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		tr.StretchMode = TextureRect.StretchModeEnum.Scale;

		var node = AvatarList[seatIndex];
		if (node != null)
		{
			int currentCards = 0;
			foreach (var c in node.GetChildren()) if (c is TextureRect) currentCards++;
			node.AddChild(tr);
			tr.Position = new Vector2(50 + (currentCards * CARD_SPACING), 0);
		}
	}

	private void RevealAllCards()
	{
		foreach (var node in AvatarList) if (node != null) foreach (var c in node.GetChildren()) if (c is TextureRect) c.QueueFree();
		foreach (var playerUid in HandCache.Keys)
		{
			foreach (var card in HandCache[playerUid])
			{
				DisplayCardAnimation(playerUid, card.Suit, card.Rank, true);
			}
		}
	}

	private async void HandleJoin(string pid)
	{
		try
		{
			if (UidDisplayed.Contains(pid)) return;
			GD.Print($"[HOST] Join: {pid}");
			
			var newPlayer = await FirebaseApi.Get<PlayerClass>($"Rooms/{currentRoom.RoomId}/Players/{pid}.json?auth={UserClass.IdToken}");
			if (newPlayer == null || !AvailableSlots.Any()) return;

			int newSeat = AvailableSlots.First();
			AvailableSlots.Remove(newSeat);
			newPlayer.Seat = newSeat;

			if (!currentRoom.Players.ContainsKey(pid)) currentRoom.Players.Add(pid, newPlayer);
			else currentRoom.Players[pid] = newPlayer;

			UidDisplayed[newSeat] = pid;
			LoadAvatar(newSeat, pid);
			DisplayPlayerInfo(newSeat, newPlayer);
		}
		catch (Exception ex) { GD.PrintErr($"Err Join: {ex.Message}"); }
	}

	private void HandleLeave(string pid)
	{
		if (!currentRoom.Players.ContainsKey(pid)) return;
		int currSeat = currentRoom.Players[pid].Seat;
		AvailableSlots.Add(currSeat);
		currentRoom.Players.Remove(pid);
		UnDisplayPlayer(currSeat);
	}

	private void DisplayPlayerInfo(int seat, PlayerClass player)
	{
		AvatarList[seat].Visible = true;
		NameList[seat].Text = player.InGameName;
		MoneyList[seat].Text = player.Money.ToString();
	}

	private void UnDisplayPlayer(int seat)
	{
		AvatarList[seat].Visible = false;
		AvatarList[seat].Texture = null;
		NameList[seat].Text = null;
		MoneyList[seat].Text = null;
	}

	private async void LoadAvatar(int seat, string uid)
	{
		AvatarList[seat].Texture = await CloudinaryService.GetImageAsync(uid);
	}

	private void InitializeDeck()
	{
		DeckOfCards = new HashSet<CardClass>();
		string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
		string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
		foreach (var suit in suits)
		{
			foreach (var rank in ranks)
			{
				int val = 0;
				if (int.TryParse(rank, out int num)) val = num;
				else if (rank == "A") val = 1;
				else val = 10;
				DeckOfCards.Add(new CardClass { Suit = suit, Rank = rank, Value = val });
			}
		}
	}

	private void OnPlayerRequestAction(string action) => HandlePlayerAction(UserClass.Uid, action);
	
	private async void OnStartGame()
	{
		try
		{
			// --- LOGIC CHẶN SỐ LƯỢNG NGƯỜI CHƠI ---
			if (currentRoom.Players.Count < 2)
			{
				OS.Alert("Cần tối thiểu 1 người chơi để bắt đầu game!");
				return;
			}
			// ---------------------------------------

			HandCache.Clear();
			foreach (var p in currentRoom.Players) HandCache.Add(p.Key, new List<CardClass>());
			foreach (var node in AvatarList) if (node != null) foreach (var c in node.GetChildren()) if (c is TextureRect) c.QueueFree();
			
			btnStartGame.Visible = false;
			currentRoom.Status = RoomStatus.PLAYING;
			
			await FirebaseApi.Patch($"Rooms/{currentRoom.RoomId}/Status.json?auth={UserClass.IdToken}", JsonConvert.SerializeObject(RoomStatus.PLAYING));
			
			var startEvent = new NT106.Scripts.Models.RoomEvent { type = "start_game", user = UserClass.Uid, time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
			await FirebaseApi.Post($"Rooms/{currentRoom.RoomId}/Events.json?auth={UserClass.IdToken}", startEvent);
			
			InitializeDeck();
			await DealCardsLogic();
		}
		catch (Exception ex) { GD.PrintErr(ex.Message); btnStartGame.Visible = true; }
	}

	private async void OnLeaveRoomPressed()
	{
		if (currentRoom == null) return;
		btnLeaveRoom.Disabled = true;
		try
		{
			var deleteSuccess = await currentRoom.DeleteAsync();
			if (deleteSuccess.Item1)
			{
				EventListener.Stop();
				RoomClass.CurrentRoom = null;
				GetTree().ChangeSceneToFile(@"Scenes\CreateRoom\CreateRoom.tscn");
			}
			else
			{
				GD.PrintErr(deleteSuccess.Item2);
				btnLeaveRoom.Disabled = false;
			}
		}
		catch (Exception) { btnLeaveRoom.Disabled = false; }
	}

	private async Task UpdateUserMoney(string uid, long newMoney) 
	{
		await FirebaseApi.Patch($"Users/{uid}.json?auth={UserClass.IdToken}", JsonConvert.SerializeObject(new { Money = newMoney })); 
	}

	#endregion
}
