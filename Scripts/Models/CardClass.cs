using Godot;
using System;

public partial class CardClass
{
	public string Suit { get; set; } // "Hearts", "Diamonds", "Clubs", "Spades"
    public string Rank { get; set; } // "A", "2"..."10", "J", "Q", "K"
    public int Value { get; set; }   // Giá trị tính điểm (1-11)

}
