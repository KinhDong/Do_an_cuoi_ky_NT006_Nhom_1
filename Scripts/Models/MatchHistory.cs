namespace NT106.Scripts.Models
{
	public class MatchHistory
	{
		public string Datetime { get; set; }
		public string RoomId { get; set; }
		public PlayerResult You { get; set; }
		public PlayerResult Opponent { get; set; } // Kết quả của đối thủ
	}

	public class PlayerResult
	{
		public string PlayerInGameName { get; set; }
		public string Role { get; set; }
		public int Score { get; set; }
		public int Strength { get; set; }
		public string Result { get; set; }
		public int MoneyChange { get; set; }
	}
}
