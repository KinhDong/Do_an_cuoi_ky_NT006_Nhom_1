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

        
    }
}
