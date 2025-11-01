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

namespace NT106
{
    public partial class CreateRoom : Form
    {
        public CreateRoom()
        {
            InitializeComponent();
            // Gán sự kiện cho các check box
            checkBox1.CheckedChanged += PlayerCount_CheckedChanged;
            checkBox2.CheckedChanged += PlayerCount_CheckedChanged;
            checkBox3.CheckedChanged += PlayerCount_CheckedChanged;
            checkBox4.CheckedChanged += PlayerCount_CheckedChanged;
            checkBox5.CheckedChanged += PlayerCount_CheckedChanged;

            checkBox8.CheckedChanged += BetAmount_CheckedChanged;
            checkBox9.CheckedChanged += BetAmount_CheckedChanged;
            checkBox10.CheckedChanged += BetAmount_CheckedChanged;
        }

        // Chỉ cho phép chọn 1 checkbox số người chơi
        private void PlayerCount_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is CheckBox current)) return;

            if (current.Checked)
            {
                foreach (var cb in new[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5 })
                {
                    if (cb != current) cb.Checked = false;
                }
            }
        }

        // Chỉ cho phép chọn 1 checkbox mức cược
        private void BetAmount_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is CheckBox current)) return;

            if (current.Checked)
            {
                foreach (var cb in new[] { checkBox8, checkBox9, checkBox10 })
                {
                    if (cb != current) cb.Checked = false;
                }
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {

        }
        private async void btn_CreateRoom_Click(object sender, EventArgs e)
        {
            try
            {
                btn_CreateRoom.Enabled = false;

                // Lấy giá trị số người chơi được chọn
                var selectedPlayerCount = new[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5 }
                    .FirstOrDefault(cb => cb.Checked)?.Text;

                // Lấy giá trị mức cược được chọn
                var selectedBet = new[] { checkBox8, checkBox9, checkBox10 }
                    .FirstOrDefault(cb => cb.Checked)?.Text;

                if (selectedPlayerCount == null || selectedBet == null)
                {
                    MessageBox.Show("Vui lòng chọn số người chơi và mức cược!",
                        "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Gọi hàm tạo phòng (RoomClass.CreateAsync)
                var room = await RoomClass.CreateAsync();

                if (room != null)
                {
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
                else
                {
                    MessageBox.Show("Không thể tạo phòng. Vui lòng thử lại!",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo phòng: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn_CreateRoom.Enabled = true;
            }
        }

        private void btn_Back_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}



