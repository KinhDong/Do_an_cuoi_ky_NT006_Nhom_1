using Godot;
using System;
using NT106.Scripts.Models;
using System.Collections.Generic;
using NT106.Scripts.Services;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

public partial class PlayAsPlayerScreen : Node2D
{
	#region --- VARIABLES & NODES ---

	// Data
	public RoomClass currentRoom { get; set; }
	public HashSet<int> AvailableSlots { get; set; } = new HashSet<int>();
	private List<CardClass> MyCards = new List<CardClass>();
	private Dictionary<string, List<CardClass>> HandCache = new Dictionary<string, List<CardClass>>();

	// UI Nodes
	public List<TextureRect> AvatarList { get; set; } = new() { null, null, null, null };
	public List<LineEdit> NameList { get; set; } = new() { null, null, null, null };
	public List<LineEdit> MoneyList { get; set; } = new() { null, null, null, null };
	public List<string> UidDisplayed { get; set; } = new() { null, null, null, null };

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
		GD.Print($"[PLAYER START] ID: {UserClass.Uid}");

		if (RoomClass.CurrentRoom == null)
		{
			GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");
			return;
		}

		currentRoom = RoomClass.CurrentRoom;

		SetupUIReferences();
		SetupPlayers();
		SetupButtons();
		SetupNetwork();
	}

	private void SetupUIReferences()
	{
		for (int i = 0; i < 4; i++)
		{
			AvatarList[i] = GetNode<TextureRect>($"pn_Background/ttr_Avatar_Player{i}");
			NameList[i] = GetNode<LineEdit>($"pn_Background/ttr_Avatar_Player{i}/le_InGameName_You");
			MoneyList[i] = GetNode<LineEdit>($"pn_Background/ttr_Avatar_Player{i}/le_Money");
		}

		txtRoomId = GetNode<LineEdit>("pn_Background/le_RoomId");
		txtBetAmount = GetNode<LineEdit>("pn_Background/le_BetAmount");

		txtRoomId.Text = currentRoom.RoomId;
		txtBetAmount.Text = currentRoom.BetAmount.ToString();
	}

	private void SetupPlayers()
	{
		// 1. Host ngồi ghế 0
		UidDisplayed[0] = currentRoom.HostId;
		LoadAvatar(0, currentRoom.HostId);
		currentRoom.Players[currentRoom.HostId].Seat = 0;
		DisplayPlayerInfo(0, currentRoom.Players[currentRoom.HostId]);

		// 2. Tôi (Player) ngồi ghế 1
		UidDisplayed[1] = UserClass.Uid;
		AvatarList[1].Texture = UserClass.Avatar;
		DisplayPlayerInfo(1, currentRoom.Players[UserClass.Uid]);

		// 3. Các người chơi khác
		int seatIndex = 2;
		foreach (var p in currentRoom.Players)
		{
			if (p.Key != UserClass.Uid && p.Key != currentRoom.HostId)
			{
				UidDisplayed[seatIndex] = p.Key;
				currentRoom.Players[p.Key].Seat = seatIndex;
				LoadAvatar(seatIndex, p.Key);
				DisplayPlayerInfo(seatIndex, currentRoom.Players[p.Key]);
				seatIndex++;
			}
		}

		// Đánh dấu ghế trống còn lại
		AvailableSlots = new();
		while (seatIndex < 4)
		{
			AvailableSlots.Add(seatIndex);
			seatIndex++;
		}
	}

	private void SetupButtons()
	{
		btnLeaveRoom = GetNode<Button>("pn_Background/btn_LeaveRoom");
		btnLeaveRoom.Pressed += OnLeaveRoomPressed;

		btnHit = GetNode<Button>("pn_Background/btn_Hit");
		btnStand = GetNode<Button>("pn_Background/btn_Stand");

		if (btnHit != null)
		{
			if (btnHit.IsConnected("pressed", new Callable(this, nameof(OnBtnHitPressed))))
				btnHit.Disconnect("pressed", new Callable(this, nameof(OnBtnHitPressed)));
			btnHit.Pressed += OnBtnHitPressed;
			btnHit.Visible = false;
		}

		if (btnStand != null)
		{
			if (btnStand.IsConnected("pressed", new Callable(this, nameof(OnBtnStandPressed))))
				btnStand.Disconnect("pressed", new Callable(this, nameof(OnBtnStandPressed)));
			btnStand.Pressed += OnBtnStandPressed;
			btnStand.Visible = false;
		}
	}

	private void SetupNetwork()
	{
		EventListener = new(FirebaseApi.BaseUrl, $"Rooms/{currentRoom.RoomId}/Events", UserClass.IdToken);
		EventListener.OnConnected += () => GD.Print("[PLAYER] Connected!");
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
			catch (Exception ex)
			{
				GD.PrintErr($"[PLAYER ERROR] {ex.Message}");
			}
		};
		EventListener.Start();
	}

	#endregion

	#region --- EVENT PROCESSING ---

	private async Task ProcessEvent(string pid, string type, dynamic evtData)
	{
		switch (type)
		{
			case "join":
				CallDeferred(nameof(HandleJoin), pid);
				break;
			case "leave":
				CallDeferred(nameof(HandleLeave), pid);
				break;
			case "deleteRoom":
				CallDeferred(nameof(HandleRoomDeleted));
				break;
			case "start_game":
				MyCards.Clear();
				HandCache.Clear();
				CallDeferred(nameof(ClearTable));
				break;
			case "deal":
				await HandleDealEvent(pid, evtData);
				break;
			case "change_turn":
				string nextUser = evtData.user;
				GD.Print($"[PLAYER EVENT] change_turn -> {nextUser}");
				CallDeferred(nameof(CheckMyTurn), nextUser);
				break;
			case "end_game":
				CallDeferred(nameof(RevealAllCards));
				CallDeferred(nameof(HandleEndGame), evtData.results);
				break;
		}
	}

	private async Task HandleDealEvent(string pid, dynamic evtData)
	{
		string rank = evtData.card.Rank;
		string suit = evtData.card.Suit;
		int val = (int)evtData.card.Value;
		var newCard = new CardClass { Rank = rank, Suit = suit, Value = val };

		if (pid == UserClass.Uid) MyCards.Add(newCard);

		if (!HandCache.ContainsKey(pid)) HandCache[pid] = new List<CardClass>();
		HandCache[pid].Add(newCard);

		bool isFaceUp = (pid == UserClass.Uid);
		CallDeferred(nameof(DisplayCardAnimation), pid, rank, suit, isFaceUp);

		// Hiện lại nút sau khi nhận bài
		if (pid == UserClass.Uid)
		{
			await Task.Delay(500);
			CallDeferred(nameof(CheckMyTurn), UserClass.Uid);
		}
	}

	#endregion

	#region --- GAME LOGIC ---

	private void CheckMyTurn(string turnUid)
	{
		if (turnUid == UserClass.Uid)
		{
			int score = currentRoom.CalculateScore(MyCards);
			GD.Print($"-> Điểm hiện tại: {score}");

			btnStand.Visible = true;

			if (score >= 21)
			{
				btnHit.Visible = false;
				OS.Alert("Bạn đã đủ điểm (hoặc Quắc), hãy bấm Dằn!");
			}
			else
			{
				btnHit.Visible = true;
			}
		}
		else
		{
			btnHit.Visible = false;
			btnStand.Visible = false;
		}
	}

	private async void SendAction(string action)
	{
		btnHit.Visible = false;
		btnStand.Visible = false;
		GD.Print($"[PLAYER SEND] Sending '{action}'...");

		var actionEvent = new NT106.Scripts.Models.RoomEvent
		{
			type = "request_action",
			user = UserClass.Uid,
			action = action,
			time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
		};

		await FirebaseApi.Post($"Rooms/{currentRoom.RoomId}/Events.json?auth={UserClass.IdToken}", actionEvent);
	}

	private void OnBtnHitPressed()
	{
		GD.Print(">>> BẤM RÚT BÀI <<<");
		SendAction("hit");
	}

	private void OnBtnStandPressed()
	{
		GD.Print(">>> BẤM DẰN <<<");
		SendAction("stand");
	}

	private async void HandleEndGame(Dictionary<string, long> results)
	{
		btnHit.Visible = false;
		btnStand.Visible = false;
		try
		{
			string myUid = UserClass.Uid;
			long winAmount = 0;
			if (results != null && results.ContainsKey(myUid)) winAmount = results[myUid];

			string msg = winAmount > 0 ? $"THẮNG {winAmount} xu!" : (winAmount < 0 ? $"THUA {Math.Abs(winAmount)} xu!" : "HÒA!");
			OS.Alert(msg);

			// Update tiền hiển thị
			var myP = await FirebaseApi.Get<PlayerClass>($"Rooms/{currentRoom.RoomId}/Players/{myUid}.json?auth={UserClass.IdToken}");
			if (myP != null)
			{
				currentRoom.Players[myUid].Money = myP.Money;
				UserClass.Money = myP.Money;
				DisplayPlayerInfo(1, currentRoom.Players[myUid]);
			}

			foreach (var uid in UidDisplayed)
			{
				if (!string.IsNullOrEmpty(uid) && uid != myUid)
				{
					var p = await FirebaseApi.Get<PlayerClass>($"Rooms/{currentRoom.RoomId}/Players/{uid}.json?auth={UserClass.IdToken}");
					if (p != null)
					{
						int seat = currentRoom.Players[uid].Seat;
						DisplayPlayerInfo(seat, p);
					}
				}
			}
		}
		catch (Exception ex) { GD.PrintErr("Lỗi EndGame: " + ex.Message); }
	}

	#endregion

	#region --- UI & HELPERS ---

	private void DisplayCardAnimation(string uid, string rank, string suit, bool isFaceUp)
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
			x = 0; y = 4;
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

	private void ClearTable()
	{
		foreach (var node in AvatarList) if (node != null) foreach (var child in node.GetChildren()) if (child is TextureRect) child.QueueFree();
	}

	private void RevealAllCards()
	{
		ClearTable();
		foreach (var playerUid in HandCache.Keys)
		{
			foreach (var card in HandCache[playerUid])
			{
				DisplayCardAnimation(playerUid, card.Rank, card.Suit, true);
			}
		}
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

	private async void HandleJoin(string pid)
	{
		var newPlayer = await FirebaseApi.Get<PlayerClass>($"Rooms/{currentRoom.RoomId}/Players/{pid}.json?auth={UserClass.IdToken}");
		int newSeat = AvailableSlots.First();
		AvailableSlots.Remove(newSeat);
		newPlayer.Seat = newSeat;
		currentRoom.Players.Add(pid, newPlayer);
		UidDisplayed[newSeat] = pid;
		LoadAvatar(newSeat, pid);
		DisplayPlayerInfo(newSeat, newPlayer);
	}

	private void HandleLeave(string pid)
	{
		if (!currentRoom.Players.ContainsKey(pid)) return;
		int currSeat = currentRoom.Players[pid].Seat;
		AvailableSlots.Add(currSeat);
		currentRoom.Players.Remove(pid);
		UnDisplayPlayer(currSeat);
	}

	private void HandleRoomDeleted()
	{
		OS.Alert("Phòng đã bị xoá bởi chủ phòng!");
		GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");
	}

	private async void OnLeaveRoomPressed()
	{
		try
		{
			var res = await currentRoom.LeaveAsync();
			if (!res.Item1) throw new Exception(res.Item2);
			EventListener.Stop();
			GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");
		}
		catch (Exception ex)
		{
			GD.PrintErr("Lỗi: ", ex.Message);
		}
	}

	#endregion
}
