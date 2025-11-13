using Godot;
using System;
using System.Threading.Tasks;
using NT106.Scripts.Models;

public partial class PlayAsBookmaker : Node2D
{
    public RoomClass room {get; set;}
    private Button Exit;
    private LineEdit RoomID; // Nên dùng Label thay vì LineEdit để hiển thị

    public override void _Ready()
    {
         room = RoomClass.CurrentRoom;
		 if (room == null)
		 {
			GetTree().ChangeSceneToFile("res://Scenes/CreateRoom/CreateRoom.tscn");
            return;
		 }

        Exit = GetNode<Button>("PlayAsBookMakerScreen/btnExitRoom");
        Exit.Pressed += ExitAndDeleteRoomFromDatabase;

        RoomID = GetNode<LineEdit>("PlayAsBookMakerScreen/txtRoomID"); 
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (room != null && RoomID != null)
        {
            RoomID.Text = $"Mã phòng: {room.RoomId}";
        }
    }

    private async void ExitAndDeleteRoomFromDatabase()
    {
    if (room == null) return;

    Exit.Disabled = true;
    GD.Print($"Đang xóa phòng: {room.RoomId}");

    try
    {
        bool deleteSuccess = await room.DeleteAsync();

        if (deleteSuccess)
        {
            GD.Print("Xóa phòng thành công!");
            
            // XÓA KHỎI STATIC VARIABLE
            RoomClass.CurrentRoom = null;
            GetTree().ChangeSceneToFile("res://Scenes/CreateRoom/CreateRoom.tscn");
        }
        else
        {
            GD.PrintErr("❌ Không thể xóa phòng!");
            Exit.Disabled = false;
        }
    }
    catch (Exception ex)
    {
        GD.PrintErr($"❌ Lỗi khi xóa phòng: {ex.Message}");
        Exit.Disabled = false;
    }
 }
}