using NT106.Forms;
using NT106.Models;

namespace NT106
{
    public partial class PlayAsPlayer : Form
    {
        private RoomClass room; // phòng hiện tại
        private readonly HttpClient http = new HttpClient();
        private CancellationTokenSource cts;

        public PlayAsPlayer(RoomClass room)
        {
            InitializeComponent();
            this.room = room;
            tb_RoomCode.Text = room.RoomId;
        }

        // Gọi khi form load để hiển thị dữ liệu ban đầu
        private void PlayAsPlayer_Load(object sender, EventArgs e)
        {
            DisplayRoom(room);
            Task.Run(() => room.ListenRoomChangesAsync(OnRoomUpdated, OnRoomDeleted));
        }

        //Call back khi phòng được cập nhật
        private void OnRoomUpdated(RoomClass updatedRoom)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnRoomUpdated(updatedRoom)));
                return;
            }
            DisplayRoom(updatedRoom);
        }

        // Phòng bị xóa 
        private void OnRoomDeleted()
        {
            Invoke((Action)(() =>
            {
                MessageBox.Show("Chủ phòng đã rời đi. Phòng đã bị giải tán!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                room.StopListening();

                // Trở về Form Room
                Room f = new Room();
                f.StartPosition = FormStartPosition.Manual;
                f.Location = this.Location;

                f.Show();
                this.Hide();
            }));
        }

        private async void btn_LeaveRoom_Click(object sender, EventArgs e)
        {
            room.StopListening();
            bool success = await room.LeaveAsync();

            if (!success)
                MessageBox.Show("Lỗi khi rời phòng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // Trở về Form Room
            Room f = new Room();
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;

            f.Show();
            this.Hide();
        }

        // Nhấn X
        private void PlayAsPlayer_FormClosing(object sender, FormClosingEventArgs e)
        {
            room.StopListening();

            AllForm.HandleFormClosing(sender, e);
        }

        // Ẩn tất cả slot trước khi hiển thị lại
        private void HideAllSlots()
        {
            for (int i = 1; i <= 6; i++)
            {
                //vì chỉ có ảnh nên sẽ ẩn ảnh, khi nào cần nhiều thông tin (money, name...) thì sẽ ẩn cả panel
                var pic = this.Controls.Find($"picP{i}", true).FirstOrDefault() as PictureBox;
                if (pic != null)
                    pic.Visible = false;
            }
        }

        // Hiển thị avatar player trong PictureBox (cần thì thêm các thông tin khác)
        private async void ShowPlayer(PictureBox pic, PlayerClass player)
        {
            if (pic == null) return;

            //Ẩn PictureBox nếu không có người chơi (hoặc người chơi rời đi)
            if (player == null)
            {
                pic.Visible = false;
                return;
            }

            //Bật PictureBox và xóa ảnh cũ
            pic.Visible = true;
            pic.Image = null;

            try
            {
                pic.Image = await UserClass.GetAvatarFromUid(player.Uid);
            }
            catch (Exception ex)
            {
                // Thêm player.InGameName để biết lỗi của ai
                Console.WriteLine($"Lỗi tải avatar cho {player.InGameName}: {ex.Message}");
            }
        }

        // Hiển thị tất cả player
        private void DisplayRoom(RoomClass room)
        {
            HideAllSlots();
            if (room?.Players == null || room.Players.Count == 0) return;

            foreach (var kvp in room.Players)
            {
                if (kvp.Value != null && kvp.Value.Uid == null)
                {
                    kvp.Value.Uid = kvp.Key;
                }
            }
            var host = room.Players.Values.FirstOrDefault(p => p.Uid == room.HostUid);
            var me = room.Players.Values.FirstOrDefault(p => p.Uid == UserClass.Uid);
            var others = room.Players.Values.Where(p => p.Uid != room.HostUid && p.Uid != UserClass.Uid).ToList();

            // Slot của mình luôn là P1
            var picMe = (PictureBox)this.Controls.Find("picP1", true).FirstOrDefault();
            ShowPlayer(picMe, me);

            // Host luôn đối diện, Slot4
            var picHost = (PictureBox)this.Controls.Find("picP4", true).FirstOrDefault();
            ShowPlayer(picHost, host);

            // Các player khác xếp slot 2,3,5,6
            int[] playerSlots = { 2, 3, 5, 6 };
            for (int i = 0; i < others.Count && i < playerSlots.Length; i++)
            {
                var pic = (PictureBox)this.Controls.Find($"picP{playerSlots[i]}", true).FirstOrDefault();
                ShowPlayer(pic, others[i]);
            }
        }
    }
}
