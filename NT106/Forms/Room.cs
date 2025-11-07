using NT106.Forms; 
using NT106.Models;   

namespace NT106
{
    public partial class Room : Form
    {
        public Room()
        {
            InitializeComponent();
            this.FormClosing += AllForm.HandleFormClosing;
        }

        private void btn_CreateNewRoom_Click(object sender, EventArgs e)
        {
            CreateRoom f = new CreateRoom();
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;

            f.Show();
            this.Hide();
        }

        private void btn_Back_Click(object sender, EventArgs e)
        {
            Menu f = new Menu();
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;

            f.Show();
            this.Hide();
        }

        private async void btn_JoinRoom_Click(object sender, EventArgs e)
        {
            string roomId = tb_RoomCode.Text.Trim();
            if (string.IsNullOrEmpty(roomId))
            {
                MessageBox.Show("Vui lòng nhập mã phòng!");
                return;
            }

            try
            {
                // Gọi hàm JoinAsync trong RoomClass
                var room = await RoomClass.JoinAsync(roomId);
                if (room == null)
                {
                    MessageBox.Show("Không thể tham gia phòng! Mã phòng có thể không tồn tại hoặc đã đầy.");
                    return;
                }

                // Mở Form PlayAsPlayer
                PlayAsPlayer f = new PlayAsPlayer(room);
                f.StartPosition = FormStartPosition.Manual;
                f.Location = this.Location;

                f.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tham gia phòng: {ex.Message}");
            }
        }        
    }
}