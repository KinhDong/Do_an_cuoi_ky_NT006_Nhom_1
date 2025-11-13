using Godot;
using System;
using NT106.Scripts.Models;
using System.Linq;
using System.Threading;

public partial class CreateRoom : Node2D
{
	Button CreateRoomWithCondition;
	Button Return;

	CheckBox TwoPlayers;
	CheckBox ThreePlayers;
	CheckBox FourPlayers;
	CheckBox FivePlayers;
	CheckBox SixPlayers;
	CheckBox Bet10;
	CheckBox Bet20;
	CheckBox Bet50;
	
	public override void _Ready()
    {
		//Nút "Tạo phòng"
        CreateRoomWithCondition = GetNode<Button>("Background/btnCreateRoom");
		CreateRoomWithCondition.Pressed += PlayAsBookMaker;

		//Nút "Quay về"
		Return = GetNode<Button>("Background/btnReturn");
		Return.Pressed += GoBackToCreateOrJoin;

		//Các lựa chọn "Số lượng người chơi"
		TwoPlayers = GetNode<CheckBox>("NumberOfPlayer/cbTwoPlayers");
		ThreePlayers = GetNode<CheckBox>("NumberOfPlayer/cbThreePlayers");
		FourPlayers = GetNode<CheckBox>("NumberOfPlayer/cbFourPlayers");
		FivePlayers = GetNode<CheckBox>("NumberOfPlayer/cbFivePlayers");
		SixPlayers = GetNode<CheckBox>("NumberOfPlayer/cbSixPlayers");

		//Chỉ chọn duy nhất một "Số lượng người chơi"
		ButtonGroup NumberOfPlayerGroup = new ButtonGroup();
		TwoPlayers.ButtonGroup = NumberOfPlayerGroup;
		ThreePlayers.ButtonGroup = NumberOfPlayerGroup;
		FourPlayers.ButtonGroup = NumberOfPlayerGroup;
		FivePlayers.ButtonGroup = NumberOfPlayerGroup;
		SixPlayers.ButtonGroup = NumberOfPlayerGroup;

		//Các lựa chọn "Mức cược"
		Bet10 = GetNode<CheckBox>("BetAmmount/cbBet10");
		Bet20 = GetNode<CheckBox>("BetAmmount/cbBet20");
		Bet50 = GetNode<CheckBox>("BetAmmount/cbBet50");

		//Chỉ chọn duy nhất một "Mức cược"
		ButtonGroup BetAmmountGroup = new ButtonGroup();
		Bet10.ButtonGroup = BetAmmountGroup;
		Bet20.ButtonGroup = BetAmmountGroup;
		Bet50.ButtonGroup = BetAmmountGroup;
    }

	//Quay về mục "Tạo hoặc Tham gia phòng"
	private void GoBackToCreateOrJoin()
    {
        GetTree().ChangeSceneToFile("res://Scenes/CreateOrJoinRoomScreen/CreateOrJoinRoom.tscn");
    }

	private int GetNumberOfPlayerFromCheckBox(CheckBox checkBox)
    {
        if (checkBox == TwoPlayers) return 2;
		if (checkBox == ThreePlayers) return 3;
		if (checkBox == FourPlayers) return 4;
		if (checkBox == FivePlayers) return 5;
		if (checkBox == SixPlayers) return 6;
		return 2;
    }

	private int GetBetAmmountFromCheckBox(CheckBox checkBox)
    {
        if (checkBox == Bet10) return 10;
		if (checkBox == Bet20) return 20;
		if (checkBox == Bet50) return 50;
		return 10;
    }

	private void ShowSuccess(string message)
    {
        GD.Print($"Thành công: {message}");
        
        // Có thể sử dụng native Godot popup hoặc custom UI
        var alert = new AcceptDialog();
        alert.Title = "Thành công";
        alert.DialogText = message;
        GetTree().Root.AddChild(alert);
        alert.PopupCentered();
    }

	 private void ShowError(string title, string message)
    {
        // Sử dụng AlertDialog hoặc hiển thị trong Godot
        GD.PrintErr($"{title}: {message}");
        
        // Có thể sử dụng native Godot popup hoặc custom UI
        var alert = new AcceptDialog();
        alert.Title = title;
        alert.DialogText = message;
        GetTree().Root.AddChild(alert);
        alert.PopupCentered();
    }

	private async void PlayAsBookMaker()
    {
        try
        {
            CreateRoomWithCondition.Disabled = true;
			
			//Lấy giá trị số lượng người chơi được chọn
			var cbNumberOfPlayer = new[] {TwoPlayers, ThreePlayers, FourPlayers, FivePlayers, SixPlayers};
			var selectedcbNOP = cbNumberOfPlayer.FirstOrDefault(cb => cb.ButtonPressed);

			//Lấy giá trị mức cược được chọn
			var cbBetAmmount = new[] {Bet10, Bet20,Bet50};
			var selectedcbBA = cbBetAmmount.FirstOrDefault(cb => cb.ButtonPressed);

			if (selectedcbNOP == null || selectedcbBA == null)
            {
				ShowError("Không thể tạo phòng!", "Vui lòng chọn đầy đủ thông tin!");
                return;
            }

			//Lấy số người chơi từ checkbox
			int maxPlayers = GetNumberOfPlayerFromCheckBox(selectedcbNOP);
			int betAmmount = GetBetAmmountFromCheckBox(selectedcbBA);

			var room = await RoomClass.CreateAsync(maxPlayers, betAmmount);
			if (room == null)
            {
				ShowError("Lỗi", "Không thể tạo phòng!");
                return;
            }

			ShowSuccess($"Tạo phòng thành công!\nMã phòng: {room.RoomId}\n" +
                       $"Người chơi tối đa: {maxPlayers}\nMức cược: {betAmmount}");
			
			RoomClass.CurrentRoom = room;
			GetTree().ChangeSceneToFile("res://Scenes/PlayAsBookmaker/PlayAsBookmaker.tscn");

        }
		catch (Exception ex)
        {
            ShowError("Lỗi khi tạo phòng", ex.Message);
        }
		finally
        {
            CreateRoomWithCondition.Disabled = false;
        }
    }
}
