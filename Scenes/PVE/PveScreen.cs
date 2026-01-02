using Godot;
using Godot.Collections;
using NT106.Scripts.Models;
using System;
using System.Collections.Generic;

public partial class PveScreen : Node2D
{
	[Export] Button ExitButton;
	[Export] Button HitButton;
	[Export] Button StandButton;

	PlayerClass E, P; // E là máy, P là người
	[Export] DisplayPlayerInfo DisplayE, DisplayP; // Hiển thị thông tin
	[Export] AnimationPlayer anim;    

	// Hiển thị lá bài của máy và người chơi
	[Export] Array<Sprite2D> ECards;
	[Export] Array<Sprite2D> PCards;
	HashSet<(int, int)> DeckOfCards;
    [Export] public AudioStream BackgroundMusic;

	public override void _Ready()
	{
        // Phát nhạc nền
        if (BackgroundMusic != null)
        {
            AudioManager.Instance.PlayMusic(BackgroundMusic);
        }

        E = new PlayerClass
		{
			InGameName = "Environment",
			Money = 9999,
			// Avatar = Tìm ảnh nào đấy cho máy, đưa vào assets rồi load lên
			Hands = new()
		};

		P = new PlayerClass
		{
			// lấy từ UserClass
			Hands = new()
		};

		DisplayE.Display(E);
		DisplayP.Display(P);

		// Khởi tạo bộ bài
		DeckOfCards = new();
		for(int rank = 1; rank <= 13; rank++)        
			for(int suit = 1; suit <= 4; suit++)
				DeckOfCards.Add((rank, suit));
	}

	// Các chức năng cần thiết:
	// Chia bài: Bốc 1 lá rồi gán trực tiếp vào Hands,
	// chạy animation (DealPlayer0 cho máy, DealCardx với từng lá cho Player).
	// Gán các tính năng cho button Hit (chia bài) và Stand (chuyển lượt cho cái)
	// Cái rút bài chỉ cần đủ điểm là dừng
	// Kết thúc bài trả lại bài cho DeckOfCards
	// Nếu thắng thì cập nhật +5 xu lên UserClass, thua không trừ

	
}
