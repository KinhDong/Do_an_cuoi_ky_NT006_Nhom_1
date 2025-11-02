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
using static System.Net.WebRequestMethods;

namespace NT106.Forms
{
    public partial class PLayAsBookmaker : Form
    {
        public PLayAsBookmaker(RoomClass room)
        {
            InitializeComponent();
            tb_RoomCode.Text = room.RoomId;
            this.room = room;
        }

        private static readonly HttpClient http = new HttpClient();
        private const string DatabaseUrl = "https://nt106-cf479-default-rtdb.firebaseio.com/";
        private async void btn_LeaveRoom_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Bạn có chắc muốn rời phòng không?\nPhòng sẽ bị xóa và tất cả người chơi sẽ bị thoát.",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.No)
                return;

            try
            {
                btn_LeaveRoom.Enabled = false;

                // Gọi hàm xóa node phòng trên Firebase
                var res = await http.DeleteAsync($"{DatabaseUrl}Rooms/{room.RoomId}.json?auth={UserClass.IdToken}");

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Phòng đã được giải tán!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Quay lại form Menu
                    this.Hide();
                    Menu menu = new Menu();
                    menu.StartPosition = FormStartPosition.Manual;
                    menu.Location = this.Location;
                    menu.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Không thể xóa phòng. Lỗi: {res.StatusCode}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi rời phòng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn_LeaveRoom.Enabled = true;
            }
        }
    }
}
