using NT106.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NT106
{
    public partial class PlayAsPlayer : Form
    {
        private RoomClass room; // phòng hiện tại
        private static readonly HttpClient http = new HttpClient();

        public PlayAsPlayer(RoomClass room)
        {
            InitializeComponent();
            this.room = room;
            tb_RoomCode.Text = room.RoomId;
        }

        private async void btn_LeaveRoom_Click(object sender, EventArgs e)
        {
            if (room == null || string.IsNullOrEmpty(UserClass.Uid))
            {
                MessageBox.Show("Không thể xác định thông tin phòng hoặc người dùng!",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Xóa người chơi này khỏi danh sách phòng
                string playerPath = $"https://nt106-cf479-default-rtdb.firebaseio.com/Rooms/{room.RoomId}/Players/{UserClass.Uid}.json?auth={UserClass.IdToken}";
                var res = await http.DeleteAsync(playerPath);

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Bạn đã rời phòng thành công!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Quay lại menu
                    Room roomMenu = new Room();
                    roomMenu.StartPosition = FormStartPosition.Manual;
                    roomMenu.Location = this.Location;
                    roomMenu.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Không thể rời phòng! Vui lòng thử lại.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi rời phòng: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PlayAsPlayer_Load(object sender, EventArgs e)
        {

        }
    }
}
