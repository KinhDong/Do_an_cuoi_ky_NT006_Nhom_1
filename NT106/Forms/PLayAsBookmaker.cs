using NT106.Models;
using static System.Net.WebRequestMethods;

namespace NT106.Forms
{
    public partial class PLayAsBookmaker : Form
    {
        private RoomClass room;
        public PLayAsBookmaker(RoomClass room)
        {
            InitializeComponent();
            tb_RoomCode.Text = room.RoomId;
            this.room = room;
        }

        private async void btn_LeaveRoom_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Bạn có chắc muốn rời phòng không?\nPhòng sẽ bị xóa và tất cả người chơi sẽ bị thoát.",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.No)
                return;

            await room.DeleteAsync();

            // Trở về Form Room
            Room f = new Room();
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;

            f.Show();
            this.Hide();
        }
        private async void PLayAsBookmaker_Load(object sender, EventArgs e)
        {
            // 1. CỐ ĐỊNH HOST: Hiển thị thông tin từ UserClass
            pn_host.Visible = true;
            lb_namehost.Text = UserClass.InGameName;
            tb_BookermakerMoney.Text = UserClass.Money.ToString("N0");
            try
            {
                pichost.Image = await UserClass.GetAvatarFromUid(UserClass.Uid);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tải avatar host: {ex.Message}");
            }
            // Hiển thị ban đầu (nếu có dữ liệu)
            DisplayRoom(room);

            // Bắt đầu lắng nghe thay đổi realtime
            //_ = là chạy ngầm không cần chờ kết quả trả về
            Task.Run(() => room.ListenRoomChangesAsync(OnRoomUpdated, OnRoomDeleted));
        }
        //Ẩn tất cả slot trước khi bắt đầu
        private void HideAllSlots()
        {
            // Ẩn các panel player
            pnP1.Visible = false;
            pnP2.Visible = false;
            pnP3.Visible = false;
            pnP4.Visible = false;
            pnP5.Visible = false;
        }

        private async void ShowSlot(PictureBox pic, Label lbname, TextBox tbmoney, Panel pnl, PlayerClass player)
        {
            if (player == null)
                return;
            pnl.Visible = true;

            //xóa ảnh cũ 
            pic.Image = null;
            try
            {
                pic.Image = await UserClass.GetAvatarFromUid(player.Uid);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị thông tin người chơi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            lbname.Text = player.InGameName;
            tbmoney.Text = player.Money.ToString("N0");
        }

        private void OnRoomUpdated(RoomClass updatedRoom)
        {
            //đảm bao gọi trên UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnRoomUpdated(updatedRoom)));
                return;
            }
            DisplayRoom(updatedRoom);
        }

        private void OnRoomDeleted()
        {
            //đảm bao gọi trên UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(OnRoomDeleted));
                return;
            }
            MessageBox.Show("Phòng đã bị xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Trở về Form Room
            Room f = new Room();
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;
            f.Show();
            this.Hide();
        }

        //hiển thị các player trong phòng
        private void DisplayRoom(RoomClass room)
        {
            HideAllSlots();
            if (room?.Players == null || room.Players.Count == 0)
                return;

            //người chơi
            var otherPlayers = room.Players
         .Where(kvp => kvp.Key != room.HostUid) // Tìm bằng Key
         .Select(kvp => {
             kvp.Value.Uid = kvp.Key; // Đồng bộ Uid cho otherPlayers
             return kvp.Value;
         })
         .ToList();
            if (otherPlayers.Count > 0 && otherPlayers[0] != null)
                ShowSlot(picP1, lb_nameP1, P1_money, pnP1, otherPlayers[0]);
            if (otherPlayers.Count > 1 && otherPlayers[1] != null)
                ShowSlot(picP2, lb_nameP2, P2_money, pnP2, otherPlayers[1]);
            if (otherPlayers.Count > 2 && otherPlayers[2] != null)
                ShowSlot(picP3, lb_nameP3, P3_money, pnP3, otherPlayers[2]);
            if (otherPlayers.Count > 3 && otherPlayers[3] != null)
                ShowSlot(picP4, lb_nameP4, P4_money, pnP4, otherPlayers[3]);
            if (otherPlayers.Count > 4 && otherPlayers[4] != null)
                ShowSlot(picP5, lb_nameP5, P5_money, pnP5, otherPlayers[4]);

            lblRoomStatus.Text = $"{room.CurrentPlayers}/{room.MaxPlayers} - {room.Status}";

            //cập nhật tiền bookermaker theo realtime
            if (room.Players.ContainsKey(UserClass.Uid))
            {
                tb_BookermakerMoney.Text = room.Players[UserClass.Uid].Money.ToString("N0");
            }
        }

        //đóng form: dừng lắng nghe
        private void PLayAsBookmaker_FormClosing(object sender, FormClosingEventArgs e)
        {
            room.StopListening();
        }
    }
}
