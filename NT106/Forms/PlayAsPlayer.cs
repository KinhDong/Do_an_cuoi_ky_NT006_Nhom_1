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

        private void PlayAsPlayer_Load(object sender, EventArgs e)
        {
            // await room.ListenRoomChangesAsync(OnRoomUpdated, OnRoomDeleted);
        }

        // 
        private void OnRoomUpdated(RoomClass updatedRoom)
        {
            Invoke((Action)(() =>
            {
                // Thêm cập nhật danh sách Players
            }));
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
    }
}
