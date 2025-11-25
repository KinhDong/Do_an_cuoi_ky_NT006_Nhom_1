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

	private Button StartGameButton;
	//Bộ bài
	public HashSet<CardClass> DeckOfCards;

    public HashSet<int> AvilableSlot {get; set;} // Những chỗ còn trống

	// Hiển thị mã phòng và mức cược
	private LineEdit DisplayRoomId;
	private LineEdit DisplayBetAmount;


	// Rời phòng
	private Button LeaveRoom;

	FirebaseStreaming EventListener;

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


		LeaveRoom = GetNode<Button>("pn_Background/btn_LeaveRoom");
		LeaveRoom.Pressed += OnLeaveRoomPressed;

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

        //Lấy tham chiếu nút Start Game
		StartGameButton = GetNode<Button>("pn_Background/btn_StartGame");
		if(room.HostId == UserClass.Uid) //Chủ phòng mới có quyền bắt đầu trò chơi
        {
			StartGameButton.Visible = true;
            StartGameButton.Pressed += OnStartGame;
        }
		else
		{
			StartGameButton.Visible = false;
        }	

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

    //-------------------------Xử lý bắt đầu trò chơi----------------------------------------
    private async void OnStartGame()
	{
		try
		{
            room.Status = RoomStatus.PLAYING;
            //cập nhật lên firebase
            await FirebaseApi.Patch($"Rooms/{room.RoomId}/Status.json?auth={UserClass.IdToken}", JsonConvert.SerializeObject(RoomStatus.PLAYING));

			//Gửi event bắt đầu trò chơi
			var startEvent = new NT106.Scripts.Models.RoomEvent
			{
				type = "start_game",
				user = UserClass.Uid,
				time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			};
            //Ghi event lên firebase
            await FirebaseApi.Post($"Rooms/{room.RoomId}/Events.json?auth={UserClass.IdToken}", startEvent);

            //Tạo và chia bài 
            InitializeDeck(); // tạo bộ bài
            await DealCardsLogic(); //Chia bài
        }
		catch (Exception ex)
		{
			GD.PrintErr($"Lỗi khi bắt đầu trò chơi: {ex.Message}");
        }
    }

    //-------------------------Xử lý tạo bộ bài----------------------------------------
    //xài HashSet để tránh trùng bài
	private void InitializeDeck()
	{
		DeckOfCards = new HashSet<CardClass>();
		string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
		string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
		foreach (var suit in suits)
		{
			foreach (var rank in ranks)
			{
				//Tính điểm cơ bản 
				int val = 0;
				if(int.TryParse(rank, out int num))
				{
					val = num;
				}
				else if(rank == "A")
				{
					val = 11;
				}
				else
				{
					val = 10;
                }
                DeckOfCards.Add(new CardClass { Suit = suit, Rank = rank, Value = val });
			}
        }
    }

    //-------------------------Xử lý chia bài----------------------------------------
	private async Task DealCardsLogic()
	{
        //OrderBySeat để chia bài đúng thứ tự
		var activePlayers = room.Players.Values.OrderBy(p => p.Seat).ToList();

        Random rand = new Random();

        //Vòng lặp chia bài: mỗi người 2 lá
        for (int i = 0; i < 2; i++)
		{
			foreach (var player in activePlayers)
			{
				//bốc bài
				int index = rand.Next(DeckOfCards.Count);
				var card = DeckOfCards.ElementAt(index);
                DeckOfCards.Remove(card); //Xoá lá bài đã chia khỏi bộ bài

                // Thay vì chỉ lưu vào DB, ta bắn Event "deal" để UI các máy khác nhận được
                var dealEvent = new NT106.Scripts.Models.RoomEvent
                {
                    type = "deal",
                    user = player.Uid,
                    time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    card = card // Gửi kèm lá bài vừa bốc
                };

                // Post Event lên Firebase
                await FirebaseApi.Post($"Rooms/{room.RoomId}/Events.json?auth={UserClass.IdToken}", dealEvent);

                //Chờ 500ms để tạo hiệu ứng chia bài (nếu cần)
                await Task.Delay(500);
			}
		}
    }
}
