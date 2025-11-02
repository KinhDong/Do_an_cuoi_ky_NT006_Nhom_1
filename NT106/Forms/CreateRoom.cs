using NT106.Forms;
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
// using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace NT106
{
    public partial class CreateRoom : Form
    {
        public CreateRoom()
        {
            InitializeComponent();

            // Đăng xuất khi đóng form (Nhấn 'X')
            this.FormClosing += AllForm.HandleFormClosing;

            // Gán sự kiện cho các check box
            chkb_MaxPLayerCount_2.CheckedChanged += PlayerCount_CheckedChanged;
            chkb_MaxPLayerCount_3.CheckedChanged += PlayerCount_CheckedChanged;
            chkb_MaxPLayerCount_4.CheckedChanged += PlayerCount_CheckedChanged;
            chkb_MaxPLayerCount_5.CheckedChanged += PlayerCount_CheckedChanged;
            chkb_MaxPLayerCount_6.CheckedChanged += PlayerCount_CheckedChanged;

            chkb_BetAmount_10.CheckedChanged += BetAmount_CheckedChanged;
            chkb_BetAmount_20.CheckedChanged += BetAmount_CheckedChanged;
            chkb_BetAmount_50.CheckedChanged += BetAmount_CheckedChanged;
        }

        // Chỉ cho phép chọn 1 checkbox số người chơi
        private void PlayerCount_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is CheckBox current)) return;

            if (current.Checked)
            {
                foreach (var chkb in new[] { chkb_MaxPLayerCount_2, chkb_MaxPLayerCount_3, chkb_MaxPLayerCount_4, chkb_MaxPLayerCount_5, chkb_MaxPLayerCount_6 })
                {
                    if (chkb != current) chkb.Checked = false;
                }
            }
        }

        // Chỉ cho phép chọn 1 checkbox mức cược
        private void BetAmount_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is CheckBox current)) return;

            if (current.Checked)
            {
                foreach (var cb in new[] { chkb_BetAmount_10, chkb_BetAmount_20, chkb_BetAmount_50 })
                {
                    if (cb != current) cb.Checked = false;
                }
            }
        }

        private async void btn_CreateRoom_Click(object sender, EventArgs e)
        {
            try
            {
                btn_CreateRoom.Enabled = false;

                // Lấy giá trị số người chơi được chọn
                var selectedPlayerCount = new[] { chkb_MaxPLayerCount_2, chkb_MaxPLayerCount_3, chkb_MaxPLayerCount_4, chkb_MaxPLayerCount_5, chkb_MaxPLayerCount_6 }
                    .FirstOrDefault(cb => cb.Checked)?.Text;

                // Lấy giá trị mức cược được chọn
                var selectedBet = new[] { chkb_BetAmount_10, chkb_BetAmount_20, chkb_BetAmount_50 }
                    .FirstOrDefault(cb => cb.Checked)?.Text;

                if (selectedPlayerCount == null || selectedBet == null)
                {
                    throw new Exception("Thiếu thông tin");
                }

                // Gọi hàm tạo phòng (RoomClass.CreateAsync)
                var room = await RoomClass.CreateAsync(int.Parse(selectedPlayerCount), int.Parse(selectedBet));

                if (room == null)
                {
                    throw new Exception("Không thể tạo phòng");
                }
                
                MessageBox.Show($"Tạo phòng thành công!\nMã phòng: {room.RoomId}\n" +
                    $"Người chơi tối đa: {selectedPlayerCount}\nMức cược: {selectedBet}",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Chuyển sang form Nhà cái
                PLayAsBookmaker playForm = new PLayAsBookmaker(room);
                playForm.StartPosition = FormStartPosition.Manual;
                playForm.Location = this.Location;
                playForm.Show();

                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo phòng: {ex.Message}");
            }
            finally
            {
                btn_CreateRoom.Enabled = true;
            }
        }

        private void btn_Back_Click(object sender, EventArgs e)
        {
            Room f = new Room();
            f.StartPosition = FormStartPosition.Manual;
            f.Location = this.Location;

            f.Show();
            this.Hide();
        }
    }
}
