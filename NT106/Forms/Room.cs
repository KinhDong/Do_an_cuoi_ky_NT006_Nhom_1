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
    public partial class Room : Form
    {
        public Room()
        {
            InitializeComponent();
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
    }
}
