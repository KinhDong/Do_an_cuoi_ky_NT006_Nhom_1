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

            this.FormClosing += AllForm.HandleFormClosing;
        }
    }
}
