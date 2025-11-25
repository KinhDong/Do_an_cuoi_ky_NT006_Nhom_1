using Godot;
using System;
using NT106.Scripts.Models;
using System.Collections.Generic;
using CloudinaryDotNet;
using NT106.Scripts.Services;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Linq;

public partial class PlayAsPlayerScreen : Node2D
{
	public RoomClass room {get; set;}

	public List<TextureRect> DisplayAvatar {get; set;}
	public List<LineEdit> DisplayName {get; set;}
	public List<LineEdit> DisplayMoney {get; set;}
	public List<string> UidDisplayed {get; set;}
	public class GameEventData
    {
        public string type { get; set; }
    	public string user { get; set; }
    	public object data { get; set; }
    }


	public HashSet<int> AvilableSlot {get; set;} // Những chỗ còn trống

	// Hiển thị mã phòng và mức cược
	private LineEdit DisplayRoomId;
	private LineEdit DisplayBetAmount;

	// Rời phòng
	private Button LeaveRoom;

	// Nút Hit/Stand
	private Button HitButton;
	private Button StandButton;
	private Label DecisionTimerLabel;
	private Timer DecisionTimer;
	private int timeRemaining;

	// Lắng nghe realtime
	FirebaseStreaming EventListener;

	// Hiển thị các lá bài
	Sprite2D [,]DisplayCards;

	// Animation
	AnimationPlayer anim;

	// Biến để theo dõi lượt chơi hiện tại
	private string currentPlayerTurn;
	private bool isMyTurn = false;

	public override void _Ready()
	{
		room = RoomClass.CurrentRoom;

		DisplayAvatar = new() {null, null, null, null};
		DisplayName = new() {null, null, null, null};
		DisplayMoney = new() {null, null, null, null};	
		UidDisplayed = new() {null, null, null, null};	

		// Hiển thị thông tin chung
		DisplayRoomId = GetNode<LineEdit>("pn_Background/le_RoomId");
		DisplayRoomId.Text = room.RoomId;
		DisplayBetAmount = GetNode<LineEdit>("pn_Background/le_BetAmount");
		DisplayBetAmount.Text = room.BetAmount.ToString();

		// Rời phòng
		LeaveRoom = GetNode<Button>("pn_Background/btn_LeaveRoom");
		LeaveRoom.Pressed += OnLeaveRoomPressed;

		//Nút Hit/Stand
		//HitButton = GetNode<Button>("");
		//StandButton = GetNode<Button>("");
		//DecisionTimerLabel = GetNode<Label>("");
		//DecisionTimer = GetNode<Timer>("");

		//HitButton.Pressed += OnHitPressed;
		//StandButton.Pressed += OnStandPressed;
		//DecisionTimer.Timeout += OnDecisionTimerTimeout;

		// Ẩn nút Hit/Stand ban đầu
		//SetDecisionButtonsVisible(false);

		// Hiển thị các lá bài
		DisplayCards = new Sprite2D[4, 5];
		for(int i = 0; i < 4; i++)
		{
			for(int j = 0; j < 5; j++)
			{
				DisplayCards[i, j] = GetNode<Sprite2D>($"pn_Background/ttr_Table/CardsOfPlayer{i}/Card{j}");
			}
		}

		// Gán thông tin PLayer
		GetNodesForPlayers();

		// Hiển thị 
		UidDisplayed[0] = room.HostId;
		LoadAvatar(0, room.HostId); // Lấy Avatar trên Cloudinary
		room.Players[room.HostId].Seat = 0;
		Display(0, room.Players[room.HostId]);

		UidDisplayed[1] = UserClass.Uid;
		DisplayAvatar[1].Texture = UserClass.Avatar;
		Display(1, room.Players[UserClass.Uid]);

		int SeatIndex = 2;
		foreach(var p in room.Players)
		{
			if(p.Key != UserClass.Uid && p.Key != room.HostId)
			{
				UidDisplayed[SeatIndex] = p.Key;
				room.Players[p.Key].Seat = SeatIndex;
				LoadAvatar(SeatIndex, p.Key);
				Display(SeatIndex, room.Players[p.Key]);

				SeatIndex++;
			}
		}

		AvilableSlot = new();
		while(SeatIndex < 4)
		{
			AvilableSlot.Add(SeatIndex); // Lưu các vị trí trống
			SeatIndex++;
		} 

		// Thực hiện lắng nghe
		EventListener = new(FirebaseApi.BaseUrl, $"Rooms/{room.RoomId}/Events", UserClass.IdToken);
		EventListener.OnConnected += () => GD.Print("Firebase connected");
		EventListener.OnDisconnected += () => GD.Print("Firebase disconnected");
		EventListener.OnError += (msg) => GD.Print("ERR: " + msg);

		EventListener.OnData += (json) =>
		{
			var evt = json.ToObject<MessageEvent>();

			if(evt != null && evt.data != null)
			{
				_ = Update(evt.data.user, evt.data.type);	
			}
		};

		EventListener.Start();
	}

	private async void OnLeaveRoomPressed()
	{
		try
		{
			var res = await room.LeaveAsync();
			if(!res.Item1) throw new Exception(res.Item2);
			
			EventListener.Stop();
			GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");	
		}

		catch (Exception ex)
		{
			GD.PrintErr("Lỗi: ", ex.Message);
		}        
	}

	public void GetNodes(int Seat)
	{
		DisplayAvatar[Seat] = GetNode<TextureRect>($"pn_Background/ttr_Avatar_Player{Seat}");
		DisplayName[Seat] = GetNode<LineEdit>($"pn_Background/ttr_Avatar_Player{Seat}/le_InGameName_You");
		DisplayMoney[Seat] = GetNode<LineEdit>($"pn_Background/ttr_Avatar_Player{Seat}/le_Money");
	}
	public void GetNodesForPlayers()
	{
		for(int i = 0; i < 4; i++) GetNodes(i);
	}

	public void Display(int Seat, PlayerClass player)
	{
		DisplayAvatar[Seat].Visible = true;
		DisplayName[Seat].Text = player.InGameName;
		DisplayMoney[Seat].Text = player.Money.ToString();
	}

	public void UnDisplay(int Seat)
	{
		DisplayAvatar[Seat].Visible = false;
		DisplayAvatar[Seat].Texture = null;
		DisplayName[Seat].Text = null;
		DisplayMoney[Seat].Text = null;
	}

	public async void LoadAvatar(int Seat, string Uid)
	{
		DisplayAvatar[Seat].Texture = await CloudinaryService.GetImageAsync(Uid);
	}	

	private void OnError(Exception ex)
	{
		GD.PrintErr($"Firebase error: {ex.Message}");
	}

	private Task Update(string Pid, string Type)
	{
		if(Type == "join") CallDeferred(nameof(UpdateJoin), Pid);
		else if(Type == "leave") CallDeferred(nameof(UpdateLeave), Pid);
		else if(Type == "deleteRoom") CallDeferred(nameof(UpdataDelete));
		return Task.CompletedTask;
	}

	private async void UpdateJoin(string Pid)
	{
		var newPlayer = await FirebaseApi.Get<PlayerClass>($"Rooms/{room.RoomId}/Players/{Pid}.json?auth={UserClass.IdToken}");

		// Lấy chỗ
		int newSeat = AvilableSlot.First();
		AvilableSlot.Remove(newSeat);
		newPlayer.Seat = newSeat;
		room.Players.Add(Pid, newPlayer);

		// Hiển thị thông tin
		UidDisplayed[newSeat] = Pid;
		LoadAvatar(newSeat, Pid);
		Display(newSeat, newPlayer);
	}

	private void UpdateLeave(string Pid)
	{
		// Lấy lại chỗ
		int CurrSeat = room.Players[Pid].Seat;
		AvilableSlot.Add(CurrSeat);
		room.Players.Remove(Pid);
		UnDisplay(CurrSeat);
	}

	private void UpdataDelete()
	{
		OS.Alert("Phòng đã bị xoá bởi chủ phòng!");
		EventListener.Stop();
		GetTree().ChangeSceneToFile(@"Scenes\CreateOrJoinRoomScreen\CreateOrJoinRoom.tscn");	
	}

	// Show giá trị lá bài
	private void DisplayCard(int playerIndex, int cardIndex, (int, int) card)
	{
		DisplayCards[playerIndex, cardIndex].Frame = card.Item1 * 13 + card.Item2;
		DisplayCards[playerIndex, cardIndex].Visible = true;
	}

	// Chia bài cho bạn
	private void AnimDealYou(int cardIndex, (int, int) card) // CardIndex: Lá bài thứ mấy
	{
		anim.Play($"DealCard{cardIndex}");
		DisplayCard(1, cardIndex, card);
		anim.Queue("RESET"); // Quay về trạng thái ban đầu
	}

	// Chia bài cho 1 player (1 đến 3)
	private void AnimDealPlayer(int playerIndex, (int, int) card)
	{
		anim.Play($"DealPlayer{playerIndex}");
		DisplayCards[playerIndex, 0].Visible = true;
		anim.Queue("RESET");
	}

	public override void _Process(double delta)
	{
		// Cập nhật timer
		if (DecisionTimer.TimeLeft > 0)
		{
			timeRemaining = (int)DecisionTimer.TimeLeft;
			//DecisionTimerLabel.Text = $"Thời gian: {timeRemaining}s";
		}
	}

	// Xử lí các event từ Cái
	private async Task HandleGameEvent(GameEventData eventData)
    {
        switch (eventData.type)
		{
			case "hitOrStand":
				await HandleHitStandEvent(eventData);
				break;

			case "hitConfirmed":
				HandleHitConfirmed(eventData);
				break;

			case "standConfirmed":
				HandleStandConfirmed(eventData);
				break;

			case "bust":
				HandleBustEvent(eventData);
				break;

			case "autoStand":
				HandleAutoStand(eventData);
				break;

			case "playerTurnsEnd":
				HandlePlayerTurnsEnd(eventData);
				break;
		}
    }

	// Xử lí event Hit/Stand
	private async Task HandleHitStandEvent(GameEventData eventData)
    {
        if (eventData.user != UserClass.Uid) 
		{
			// Không phải lượt của mình
			currentPlayerTurn = eventData.user;
			isMyTurn = false;
			SetDecisionButtonsVisible(false);
			//DecisionTimerLabel.Text = $"Đang chờ {room.Players[eventData.user].InGameName}...";
			return;
		}

		// Lượt của mình
		currentPlayerTurn = eventData.user;
		isMyTurn = true;

		// Hiển thị thông tin bài hiện tại
		if (eventData.data != null)
		{
			var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventData.data.ToString());
			if (data.ContainsKey("currentScore"))
			{
				var currentScore = Convert.ToInt32(data["currentScore"]);
			}
		}
		
		// Hiển thị nút và bắt đầu đếm ngược
		SetDecisionButtonsVisible(true);
		DecisionTimer.Start(15);
		timeRemaining = 15;
    }

	private void HandleHitConfirmed(GameEventData eventData)
    {
        // Thêm cái hiệu ứng hay thông báo gì đấy
    }

	private void HandleStandConfirmed(GameEventData eventData)
	{
		// Thêm hiệu ứng hay thông báo gì đó hoặc khỏi
	}

	private void HandleBustEvent(GameEventData eventData)
	{
		if (eventData.data != null)
		{
			var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventData.data.ToString());
			if (data.ContainsKey("score"))
			{
				var score = Convert.ToInt32(data["score"]);
				//GD.Print($"{room.Players[eventData.user].InGameName} BUST với {score} điểm!");
			}
		}
	}

	private void HandleAutoStand(GameEventData eventData)
	{
		if (eventData.data != null)
		{
			var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventData.data.ToString());
			if (data.ContainsKey("reason") && data.ContainsKey("score"))
			{
				var reason = data["reason"].ToString();
				var score = Convert.ToInt32(data["score"]);
				//GD.Print($"{room.Players[eventData.user].InGameName} tự động Stand ({reason}) với {score} điểm");
			}
		}
	}

	private void HandlePlayerTurnsEnd(GameEventData eventData)
    {
        //GD.Print("Kết thúc lượt tất cả người chơi, chuyển sang lượt Nhà Cái");
		SetDecisionButtonsVisible(false);
		//DecisionTimerLabel.Text = "Kết thúc lượt người chơi";
    }

	// Người chơi chọn Hit
	private async void OnHitPressed()
    {
        if (!isMyTurn) return;
		SetDecisionButtonsVisible(false);
		DecisionTimer.Stop();
		
		try
        {
            // Gửi quyết định Hit lên Firebase
			await PostDecision("hit");
			//GD.Print("Đã chọn Hit");
        }
		catch (Exception ex)
		{
			GD.PrintErr($"Lỗi khi gửi Hit: {ex.Message}");
		}
    }

	// Người chơi chọn Stand
	private async void OnStandPressed()
    {
        if (!isMyTurn) return;
		SetDecisionButtonsVisible(false);
		DecisionTimer.Stop();

        try
        {
            // Gửi quyết định Stand lên Firebase
			await PostDecision("stand");
			//GD.Print("Đã chọn Stand");
        }
		catch (Exception ex)
        {
            GD.PrintErr($"Lỗi khi gửi Stand: {ex.Message}");
        }
    }

	// Hết thời gian quyết định
	private async void OnDecisionTimerTimeout()
    {
        if (!isMyTurn) return;
		SetDecisionButtonsVisible(false);
		GD.Print("Hết thời gian! Tự động quyết định");

		try
        {
            // Lấy điểm hiện tại để quyết định thông minh
			var playerCards = await GetMyCards();
			var currentScore = CalculateScore(playerCards);

			// Quyết định tự động: <16 Hit, >=16 Stand
			string decision = currentScore < 16 ? "hit" : "stand";
			await PostDecision(decision);
			//GD.Print($"Tự động chọn {decision} (điểm: {currentScore})");
        }
		catch (Exception ex)
		{
			GD.PrintErr($"Lỗi khi gửi quyết định tự động: {ex.Message}");
		}
    }

	// Gửi quyết định lên Firebase
	private async Task PostDecision(string decision)
    {
        try
        {
            var decisionEvent = new
			{
				type = "playerDecision",
				user = UserClass.Uid,
				data = new
				{
					decision = decision,
					timestamp = DateTime.UtcNow
				}
			};
			await FirebaseApi.Post($"Rooms/{room.RoomId}/Events.json?auth={UserClass.IdToken}", decisionEvent);
        }
		catch (Exception ex)
        {
            GD.PrintErr($"Lỗi khi post decision: {ex.Message}");
			throw;
        }
    }

	// Lấy bài của người chơi hiện tại
	private async Task<List<(int, int)>> GetMyCards()
	{
		try
		{
			var cards = await FirebaseApi.Get<List<(int, int)>>(
				$"Rooms/{room.RoomId}/CurrentRound/Players/{UserClass.Uid}/Cards.json?auth={UserClass.IdToken}");
			return cards ?? new List<(int, int)>();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Lỗi khi lấy bài: {ex.Message}");
			return new List<(int, int)>();
		}
	}

	// Tính điểm bài
	private int CalculateScore(List<(int, int)> cards)
	{
		int score = 0;
		int aceCount = 0;

		foreach (var card in cards)
		{
			var rank = card.Item1;

			if (rank == 1) // Ace
			{
				aceCount++;
				score += 11;
			}
			else if (rank >= 10) // 10, J, Q, K
			{
				score += 10;
			}
			else
			{
				score += rank;
			}
		}

		// Điều chỉnh Ace từ 11 xuống 1 nếu cần
		while (score > 21 && aceCount > 0)
		{
			score -= 10;
			aceCount--;
		}

		return score;
	}

	// Hiển thị/ẩn nút quyết định
	private void SetDecisionButtonsVisible(bool visible)
	{
		HitButton.Visible = visible;
		StandButton.Visible = visible;
		DecisionTimerLabel.Visible = visible;

		if (visible)
		{
			HitButton.Disabled = false;
			StandButton.Disabled = false;
		}
	}
}
