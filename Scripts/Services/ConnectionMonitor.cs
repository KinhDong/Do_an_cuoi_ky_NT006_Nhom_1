using Godot;
using System;
using System.Threading.Tasks;
using NT106.Scripts.Services;
using NT106.Scripts.Models;

public partial class ConnectionMonitor : Node
{
    private double lostTime = 0f;
    private Timer timer;
    private bool isReconnecting = false;

    public void StartMonitoring()
    {
        if (timer != null && timer.IsInsideTree())
        {
            return; // Đã bắt đầu rồi
        }

        timer = new Timer();
        timer.WaitTime = 5f;
        timer.Timeout += OnCheck;
        AddChild(timer);
        timer.Start();
    }

    public override void _Ready()
    {
        // Chỉ bắt đầu nếu đã đăng nhập
        if (string.IsNullOrEmpty(UserClass.Uid) || string.IsNullOrEmpty(UserClass.IdToken))
        {
            return;
        }

        StartMonitoring();
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
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            bool success = await FirebaseApi.Put($"Users/{UserClass.Uid}/LastHeartbeat", timestamp);
            return success;
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
