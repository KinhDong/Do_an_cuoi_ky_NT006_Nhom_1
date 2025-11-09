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

        //Ẩn tất cả slot trước khi bắt đầu
        private void HideAllSlots()
        {
            // Ẩn host
            pn_host.Visible = false;

            // Ẩn các panel player
            pnP1.Visible = false;
            pnP2.Visible = false;
            pnP3.Visible = false;
            pnP4.Visible = false;
            pnP5.Visible = false;
        }

        private void ShowSlot(PictureBox pic, Label lbname, TextBox tbmoney, Panel pnl, PlayerClass player)
        {
            if (player == null)
                return;
            pnl.Visible = true;

            try
            {
                pic.Image = player.Avatar;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị thông tin người chơi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            lbname.Text = player.InGameName;
            tbmoney.Text = player.Money.ToString("N0");
        }
        //Gọi kho form load để hiển thị dữ liệu ban đầu
        private async void PLayAsBookmaker_Load(object sender, EventArgs e)
        {
            HideAllSlots();
            // Hiển thị ban đầu (nếu có dữ liệu)
            DisplayRoom(room);

            // Bắt đầu lắng nghe thay đổi realtime
            await room.ListenRoomChangesAsync(OnRoomUpdated, OnRoomDeleted);
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
            if (room?.Players == null || room.Players.Count ==0)
                return;

            //nhà cái
            var host = room.Players.Values.FirstOrDefault(p => p.Uid == room.HostUid);
            if(host != null)
            {
                ShowSlot(pichost,lb_namehost, tb_BookermakerMoney, pn_host, host);
            }

            //người chơi
            var otherPlayers = room.Players.Values.Where(p => p.Uid != room.HostUid).ToList();
            if (otherPlayers.Count > 0 && otherPlayers[0] != null)
                ShowSlot(picP1, lb_nameP1, P1_money, pnP1, otherPlayers[0]);
            if(otherPlayers.Count > 1 && otherPlayers[1] != null)
                ShowSlot(picP2, lb_nameP2, P2_money, pnP2, otherPlayers[1]);
            if(otherPlayers.Count > 2 && otherPlayers[2] != null)
                ShowSlot(picP3, lb_nameP3, P3_money, pnP3, otherPlayers[2]);
            if(otherPlayers.Count > 3 && otherPlayers[3] != null)
                ShowSlot(picP4, lb_nameP4, P4_money, pnP4, otherPlayers[3]);
            if(otherPlayers.Count > 4 && otherPlayers[4] != null)
                ShowSlot(picP5, lb_nameP5, P5_money, pnP5, otherPlayers[4]);

            lblRoomStatus.Text = $"{room.CurrentPlayers}/{room.MaxPlayers} - {room.Status}";
        }

        //đóng form: dừng lắng nghe
        private void PLayAsBookmaker_FormClosing(object sender, FormClosingEventArgs e)
        {
            room.StopListening();
        }
    }
}
