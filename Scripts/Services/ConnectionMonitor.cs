using Godot;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

public partial class ConnectionMonitor : Node
{
    private double lostTime = 0f;
    private Timer timer;
    private bool isReconnecting = false;

    public override void _Ready()
    {
        timer = new Timer();
        timer.WaitTime = 5f;
        timer.Timeout += OnCheck;
        AddChild(timer);
        timer.Start();
    }

    private async void OnCheck()
    {
        bool connected = await CheckInternetAsync();

        if (connected)
        {
            if (isReconnecting)
            {
                OS.Alert("🔌 Reconnect thành công!");
            }

            lostTime = 0f;
            isReconnecting = false;
            return;
        }

        // Mất kết nối
        lostTime += timer.WaitTime;

        if (!isReconnecting)
        {
            isReconnecting = true;
            OS.Alert("Mất kết nối internet!");
        }

        if (lostTime >= 30f)
        {
            ForceLogout();
        }
    }

    private async Task<bool> CheckInternetAsync()
    {
        try
        {
            var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 1000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    private void ForceLogout()
    {
        OS.Alert("❌ Reconnect thất bại quá 30s → Thoát game");

        // AuthManager.Logout(); // Uncomment if needed
        GetTree().Quit();
    }
}
