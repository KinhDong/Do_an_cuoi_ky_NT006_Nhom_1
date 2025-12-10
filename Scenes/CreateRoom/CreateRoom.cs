using Godot;
using System;
using NT106.Scripts.Models;
using System.Linq;
using System.Threading;

public partial class CreateRoom : Node2D
{
	[Export] Button CreateRoomWithCondition;
	[Export] Button Return;

	[Export] CheckBox Bet10;
	[Export] CheckBox Bet20;
	[Export] CheckBox Bet50;
	
	public override void _Ready()
    {
		//Nút "Tạo phòng"
		CreateRoomWithCondition.Pressed += PlayAsBookMaker;

		//Nút "Quay về"
		Return.Pressed += GoBackToCreateOrJoin;

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


	private int GetBetAmmountFromCheckBox(CheckBox checkBox)
    {
        if (checkBox == Bet10) return 10;
		if (checkBox == Bet20) return 20;
		if (checkBox == Bet50) return 50;
		return 10;
    }

	private void ShowSuccess(string message)
    {
        var alert = new AcceptDialog();
        alert.Title = "Thành công";
        alert.DialogText = message;
        GetTree().Root.AddChild(alert);
        alert.PopupCentered();
    }

	private void ShowError(string title, string message)
    {        
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
			
			//Lấy giá trị mức cược được chọn
			var cbBetAmmount = new[] {Bet10, Bet20, Bet50};
			var selectedcbBA = cbBetAmmount.FirstOrDefault(cb => cb.ButtonPressed);

			if (selectedcbBA == null)
            {
				ShowError("Không thể tạo phòng!", "Vui lòng chọn đầy đủ thông tin!");
                return;
            }

			//Lấy số tiền cược từ checkbox
			int betAmmount = GetBetAmmountFromCheckBox(selectedcbBA);

			var room = await RoomClass.CreateAsync(betAmmount);
			if (!room.Item1)
            {
				ShowError("Lỗi", room.Item2);
                return;
            }

			ShowSuccess($"Tạo phòng thành công!");
			
			GetTree().ChangeSceneToFile(@"Scenes\PlayAsBookmaker\PlayAsBookmakerScreen.tscn");

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
