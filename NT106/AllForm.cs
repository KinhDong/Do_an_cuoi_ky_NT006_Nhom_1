using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NT106
{
    public static class AllForm
    {
        public static async void HandleFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Tự động đăng xuất khi đóng app
                await UserClass.LogoutAsync();
            }

            // Tắt hoàn toàn chương trình, ngăn ngừa chạy ngầm
            Environment.Exit(0);
        }
    }
}
