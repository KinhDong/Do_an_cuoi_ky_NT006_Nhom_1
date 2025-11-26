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
	public RoomClass room {get; set;}

	public List<TextureRect> DisplayAvatar {get; set;}
	public List<LineEdit> DisplayName {get; set;}
	public List<LineEdit> DisplayMoney {get; set;}
	public List<string> UidDisplayed {get; set;}


	public HashSet<int> AvilableSlot {get; set;} // Những chỗ còn trống

	// Hiển thị mã phòng và mức cược
	private LineEdit DisplayRoomId;
	private LineEdit DisplayBetAmount;

	// Rời phòng
	private Button LeaveRoom;

	FirebaseStreaming EventListener;

	// Hiển thị các lá bài
	Sprite2D [,]DisplayCards;

	// Animation
	AnimationPlayer anim;

	//Quản lí trạng thái trò chơi: Trạng thái Game (GameState) và Trạng thái Player (PlayerAction)
	public enum GameState { Waiting, Dealing, PlayerTurn, DealerTurn, GameOver }
	public enum PlayerAction { None, Hit, Stand, Bust }
	//
	public class GameEvent
    {
        public string type { get; set; }
    	public string user { get; set; }
    	public object data { get; set; }
    }
	private GameState currentGameState;
	private List<string> playerTurnOrder;
    private int currentPlayerIndex;
    private Dictionary<string, PlayerAction> playerActions;
    private Timer decisionTimer;//Thời gian quy định của mỗi lượt

	public override void _Ready()
	{
		room = RoomClass.CurrentRoom;

		DisplayAvatar = new() {null, null, null, null};
		DisplayName = new() {null, null, null, null};
		DisplayMoney = new() {null, null, null, null};
		UidDisplayed = new() {null, null, null, null};

		// Tạo các vị trí trống
		AvilableSlot = new HashSet<int> {1, 2, 3};			

		// Hiển thị thông tin chung
		DisplayRoomId = GetNode<LineEdit>("pn_Background/le_RoomId");
		DisplayRoomId.Text = room.RoomId;
		DisplayBetAmount = GetNode<LineEdit>("pn_Background/ttr_Table/le_BetAmount");
		DisplayBetAmount.Text = room.BetAmount.ToString();

		// Gán và hiển thị thông tin PLayer
		GetNodesForPlayers();
		DisplayAvatar[0].Texture = UserClass.Avatar;
		Display(0, room.Players[room.HostId]);

		//Khởi tạo biến cho hit và stand
		currentGameState = GameState.Waiting;
		playerTurnOrder = new List<string>();
		playerActions = new Dictionary<string, PlayerAction>();
		
		//Kết nối sự kiện timer
		//decisionTimer.Timeout += OnDecisionTimeout;

		LeaveRoom = GetNode<Button>("pn_Background/btn_LeaveRoom");
		LeaveRoom.Pressed += OnLeaveRoomPressed;

		// Hiển thị các lá bài
		DisplayCards = new Sprite2D[4, 5];
		for(int i = 0; i < 4; i++)
		{
			for(int j = 0; j < 5; j++)
			{
				DisplayCards[i, j] = GetNode<Sprite2D>($"pn_Background/ttr_Table/CardsOfPlayer{i}/Card{j}");
			}
		}

		anim = GetNode<AnimationPlayer>("pn_Background/ttr_Table/AnimationPlayer");

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
		if (room == null) return;

		LeaveRoom.Disabled = true;
		GD.Print($"Đang xóa phòng: {room.RoomId}");

		try
		{
			var deleteSuccess = await room.DeleteAsync();

			if (deleteSuccess.Item1)
			{
				GD.Print("Xóa phòng thành công!");
				
				EventListener.Stop();
				RoomClass.CurrentRoom = null;				

				GetTree().ChangeSceneToFile(@"Scenes\CreateRoom\CreateRoom.tscn");				
			}

			else
			{
				GD.PrintErr(deleteSuccess.Item2);
				LeaveRoom.Disabled = false;
			}			
		}
		
		catch (Exception ex)
		{
			GD.PrintErr($"Lỗi khi xóa phòng: {ex.Message}");
			LeaveRoom.Disabled = false;
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

	public async void LoadAvatar(int Seat, string Uid)
	{
		DisplayAvatar[Seat].Texture = await CloudinaryService.GetImageAsync(Uid);
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

	private Task Update(string Pid, string Type)
	{
		if(Type == "join") CallDeferred(nameof(UpdateJoin), Pid);
		else if(Type == "leave") CallDeferred(nameof(UpdateLeave), Pid);
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
		DisplayCard(0, cardIndex, card);
		anim.Queue("RESET"); // Quay về trạng thái ban đầu
	}

	// Chia bài cho 1 player (1 đến 3)
	private void AnimDealPlayer(int playerIndex, (int, int) card)
	{
		anim.Play($"DealPlayer{playerIndex}");
		DisplayCards[playerIndex, 0].Visible = true;
		anim.Queue("RESET");
	}

	// Bắt đầu lượt chơi hit hoặc stand (sau khi chia bài xong)
	public async void StartHitStandPhase()
    {
        currentGameState = GameState.PlayerTurn;
		
		// Thiết lập thứ tự lượt chơi (bỏ qua slot null)
		playerTurnOrder.Clear();
		for (int i = 0; i < UidDisplayed.Count; i++)
        {
            if (UidDisplayed[i] != null && room.Players.ContainsKey(UidDisplayed[i]))
			playerTurnOrder.Add(UidDisplayed[i]);
            // Khởi tạo hành động mặc định
            playerActions[UidDisplayed[i]] = PlayerAction.None;
        }

		currentPlayerIndex = 0;
		await StartNextPlayerTurn();
    }

	// Bắt đầu lượt chơi tiếp theo
	private async Task StartNextPlayerTurn()
    {
        // Kiểm tra đã hết lượt tất cả người chơi
		if (currentPlayerIndex >= playerTurnOrder.Count)
        {
            await EndPlayerTurns();
			return;
        }

		var currentPlayerId = playerTurnOrder[currentPlayerIndex];

		// Kiểm tra người chơi đã stand(dừng bốc bài) hoặc bust(quá điểm)
		if (playerActions[currentPlayerId] == PlayerAction.Stand || 
        	playerActions[currentPlayerId] == PlayerAction.Bust)
        {
            currentPlayerIndex++;
        	await StartNextPlayerTurn();
        	return;
        }

		// Kiểm tra nếu >= 21 thì tự động xử lí
		var playerCards = await GetPlayerCards(currentPlayerId);
    	var currentScore = CalculateScore(playerCards);

		if (currentScore >= 21)
        {
            // Đạt 21 điểm
			if (currentScore == 21)
            {
                playerActions[currentPlayerId] = PlayerAction.Stand;
				await PostEvent("autoStand", currentPlayerId, new { reason = "reached_21_points", score = currentScore });
            }
			else if (currentScore > 21)
            {
                playerActions[currentPlayerId] = PlayerAction.Bust;
				 await PostEvent("bust", currentPlayerId, new { score = currentScore });
            }
			currentPlayerIndex++;
        	await StartNextPlayerTurn();
        	return;
        }

		// Gửi event hit/stand cho người chơi hiện tai
		await PostHitStandEvent(currentPlayerId);

		//Bắt đầu timer
		decisionTimer.Start(15);
    }

	// Gửi event hit stand cho người chơi
	private async Task PostHitStandEvent(string playerId)
    {
        // Lấy thông tin bài hiện tại của người chơi từ Firebase
    	var playerCards = await GetPlayerCards(playerId);
    	var currentScore = CalculateScore(playerCards);

		var eventData = new
        {
          	action = "hitOrStand",
        	playerId = playerId,
        	currentCards = playerCards,
        	currentScore = currentScore,
        	timeout = 15 // thời gian chờ tính bằng giây
        };

		await PostEvent("hitOrStand", playerId, eventData);
    }

	// Xử lí hit stand từ người chơi
	private async void HandlePlayerDecision(string playerId, string decision)
    {
        if (currentGameState != GameState.PlayerTurn) return;
		var currentPlayerId = playerTurnOrder[currentPlayerIndex];

		// Chỉ chấp nhận thao tác từ người chơi hiện tại
		if (playerId != currentPlayerId) 
    	{
        	return;
    	}
		
		//Dừng timer
		decisionTimer.Stop();

		switch (decision.ToLower())
    	{
        	case "hit":
            	await ProcessHitAction(playerId);
            	break;
            
        	case "stand":
            	await ProcessStandAction(playerId);
            	break;
            
        	default:
            	// Mặc định là Stand nếu quyết định không hợp lệ
            	await ProcessStandAction(playerId);
            	break;
    	}
    }

	// Người chơi chọn Hit
	private async Task ProcessHitAction(string playerId)
    {
        playerActions[playerId] = PlayerAction.Hit;
		// Gửi event xác nhận hit
    	await PostEvent("hitConfirmed", playerId, new { action = "hit" });

		// Sau khi chia bài, kiểm tra điểm
		var playerCards = await GetPlayerCards(playerId);
    	var newScore = CalculateScore(playerCards);

		// Kiểm tra bust
		if (newScore > 21)
        {
            playerActions[playerId] = PlayerAction.Bust;
        	await PostEvent("bust", playerId, new { score = newScore });
        	GD.Print($"{room.Players[playerId].InGameName} BUST với điểm: {newScore}");
        
        	// Chuyển sang người chơi tiếp theo
        	currentPlayerIndex++;
        	await StartNextPlayerTurn();
        }
		else
        {
            // Tiếp tục gửi event hit/stand cho cùng người chơi
        	await PostHitStandEvent(playerId);
        	decisionTimer.Start(15);
        }
    }

	// Người chơi chọn Stand
	private async Task ProcessStandAction(string playerId)
    {
        playerActions[playerId] = PlayerAction.Stand;

		// Gửi event xác nhận stand
		await PostEvent("standConfirmed", playerId, new { action = "stand" });

		// Chuyển sang người chơi tiếp theo
    	currentPlayerIndex++;
    	await StartNextPlayerTurn();
    }

	// Khi hết thời gian quyết định lựa chọn
	private async void OnDecisionTimeout()
	{
    	if (currentGameState == GameState.PlayerTurn)
    	{
        	var currentPlayerId = playerTurnOrder[currentPlayerIndex];

			// Kiểm tra điểm sau khi chia bài để lựa chọn mặc định sau khi hết timer
			var playerCards = await GetPlayerCards(currentPlayerId);
        	var currentScore = CalculateScore(playerCards);

			// < 16 mặc định hit, >= 16 mặc định stand
			if (currentScore < 16)
            {
				// Mặc định là Hit nếu hết giờ (< 16)
                await ProcessHitAction(currentPlayerId);
            } 
			else 
			{
        		// Mặc định là Stand nếu hết giờ (>= 16)
        		await ProcessStandAction(currentPlayerId);
			}
    	}
	}

	// Kết thúc lượt tất cả người chơi
	private async Task EndPlayerTurns()
    {
        currentGameState = GameState.DealerTurn;

		// Gửi event thông báo kết thúc lượt người chơi
    	await PostEvent("playerTurnsEnd", null, new { 
        	playerActions = playerActions 
    	});

		// Chuyển sang lượt của Nhà cái
    	await StartDealerTurn();
    }

	// Lượt của Cái
	private async Task StartDealerTurn()
    {
		// Lấy bài của Cái
        var dealerCards = await GetDealerCards();
		int dealerScore = CalculateScore(dealerCards);

		// Gửi event bắt đầu lượt của Cái
		await PostEvent("dealerTurnStart", null, new
        {
            dealerScore = dealerScore,
			dealerCards = dealerCards
        });

        //Luật áp dụng tương tự với Cái
        while (dealerScore < 16)
        {
			var newCard = await DrawCardForDealer();
			if (newCard != (0, 0))
            {
                dealerCards.Add(newCard);
            	dealerScore = CalculateScore(dealerCards);

				// Gửi event Nhà Cái rút bài
            	await PostEvent("dealerHit", null, new {
                	newCard = newCard,
                	dealerCards = dealerCards,
                	dealerScore = dealerScore
            	});
				
				// Kiểm tra Bust
				if (dealerScore > 21)
                {
                    await PostEvent("dealerBurst", null, new
                    {
                       dealerScore = dealerScore 
                    });
					//GD.Print($"Nhà Cái BUST với {dealerScore} điểm!");
					break;
                }
            }
			else
            {
                break;
            }	
        }

		// Kết thúc lượt của Cái
		await PostEvent("dealerTurnEnd", null, new
        {
            finalDealerScore = dealerScore,
        	isBust = dealerScore > 21
        });
		
		//GD.Print($"Nhà Cái kết thúc với {dealerScore} điểm");
		currentGameState = GameState.GameOver;
    }

	// Rút bài cho Cái 
	private async Task<(int, int)> DrawCardForDealer()
    {
		try
        {
            // Giả sử có deck bài
        	var deck = await FirebaseApi.Get<List<(int, int)>>($"Rooms/{room.RoomId}/CurrentRound/Deck.json?auth={UserClass.IdToken}");
        
        	if (deck != null && deck.Count > 0)
        	{
            	var card = deck[0];
            	// Xóa bài đã rút
            	deck.RemoveAt(0);
            	await FirebaseApi.Put($"Rooms/{room.RoomId}/CurrentRound/Deck.json?auth={UserClass.IdToken}", deck);
            
            	// Thêm bài cho Nhà Cái
            	var dealerCards = await GetDealerCards();
            	dealerCards.Add(card);
            	await FirebaseApi.Put($"Rooms/{room.RoomId}/CurrentRound/Dealer/Cards.json?auth={UserClass.IdToken}", dealerCards);
            
            	return card;
        	}
        }
		catch (Exception ex)
    	{
        	GD.PrintErr($"Lỗi khi rút bài cho Nhà Cái: {ex.Message}");
    	}
        return(0, 0);
    }

	// Lấy bài của Cái
	private async Task<List<(int, int)>> GetDealerCards()
    {
       try
    	{
        	var cards = await FirebaseApi.Get<List<(int, int)>>(
            $"Rooms/{room.RoomId}/CurrentRound/Dealer/Cards.json?auth={UserClass.IdToken}");
        	return cards ?? new List<(int, int)>();
    	}
    	catch (Exception ex)
    	{
        	GD.PrintErr($"Lỗi khi lấy bài Nhà Cái: {ex.Message}");
        	return new List<(int, int)>();
    	}
    }

	// Lấy bài của người chơi từ Firebase
	private async Task<List<(int, int)>> GetPlayerCards(string playerId)
	{
    	try
    	{
        	var cards = await FirebaseApi.Get<List<(int, int)>>(
            $"Rooms/{room.RoomId}/CurrentRound/Players/{playerId}/Cards.json?auth={UserClass.IdToken}");
        	return cards ?? new List<(int, int)>();
    	}
    	catch (Exception ex)
    	{
        	GD.PrintErr($"Lỗi khi lấy bài của người chơi {playerId}: {ex.Message}");
        	return new List<(int, int)>();
    	}
	}
	
	// Tính điểm
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

	// Post event lên Firebase
	private async Task PostEvent(string type, string user, object data)
	{
    	try
    	{
        	var evt = new GameEvent
        	{	
            	type = type,
            	user = user,
            	data = data
        	};
        
        	await FirebaseApi.Post($"Rooms/{room.RoomId}/Events.json?auth={UserClass.IdToken}", evt);
    	}	
    	catch (Exception ex)
    	{
        	GD.PrintErr($"Lỗi khi post event: {ex.Message}");
    	}
	}
}
