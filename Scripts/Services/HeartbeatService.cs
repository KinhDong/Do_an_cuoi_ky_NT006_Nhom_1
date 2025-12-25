using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NT106.Scripts.Services;
using NT106.Scripts.Models;

namespace NT106.Scripts.Services
{
    public partial class HeartbeatService : Node
    {
        private Timer heartbeatTimer;
        private Timer checkTimer;
        private string roomId;
        private bool isHost;
        private Dictionary<string, DateTime> lastHeartbeats = new();

        public void StartHeartbeat(string _roomId, bool _isHost)
        {
            roomId = _roomId;
            isHost = _isHost;

            // Timer để gửi heartbeat mỗi 5s
            heartbeatTimer = new Timer();
            heartbeatTimer.WaitTime = 5f;
            heartbeatTimer.Timeout += SendHeartbeat;
            AddChild(heartbeatTimer);
            heartbeatTimer.Start();

            // Timer để check heartbeat mỗi 5s
            checkTimer = new Timer();
            checkTimer.WaitTime = 5f;
            checkTimer.Timeout += CheckHeartbeats;
            AddChild(checkTimer);
            checkTimer.Start();

            // Gửi heartbeat đầu tiên
            SendHeartbeat();
        }

        public void StopHeartbeat()
        {
            heartbeatTimer?.Stop();
            checkTimer?.Stop();
        }

        private async void SendHeartbeat()
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                await FirebaseApi.Put($"Rooms/{roomId}/Heartbeat/{UserClass.Uid}", timestamp);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error sending heartbeat: {ex.Message}");
            }
        }

        private async void CheckHeartbeats()
        {
            try
            {
                if (isHost)
                {
                    // Host checks all players' heartbeats
                    var heartbeatJson = await FirebaseApi.GetRaw($"Rooms/{roomId}/Heartbeat");
                    if (heartbeatJson != "null")
                    {
                        var heartbeats = JsonConvert.DeserializeObject<Dictionary<string, string>>(heartbeatJson);
                        var now = DateTime.UtcNow;

                        foreach (var kvp in heartbeats)
                        {
                            var playerId = kvp.Key;
                            var timestampStr = kvp.Value;
                            if (DateTime.TryParse(timestampStr, out var timestamp))
                            {
                                if ((now - timestamp).TotalSeconds > 30)
                                {
                                    // Player disconnected, remove from room
                                    await RemovePlayerFromRoom(playerId);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Non-host checks host's heartbeat
                    var hostHeartbeatJson = await FirebaseApi.GetRaw($"Rooms/{roomId}/Heartbeat/{RoomClass.CurrentRoom.HostId}");
                    if (hostHeartbeatJson == "null" || string.IsNullOrEmpty(hostHeartbeatJson))
                    {
                        // Host disconnected, leave room
                        await HandleHostDisconnected();
                    }
                    else
                    {
                        var timestampStr = JsonConvert.DeserializeObject<string>(hostHeartbeatJson);
                        if (DateTime.TryParse(timestampStr, out var timestamp))
                        {
                            if ((DateTime.UtcNow - timestamp).TotalSeconds > 30)
                            {
                                await HandleHostDisconnected();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error checking heartbeats: {ex.Message}");
            }
        }

        private async Task RemovePlayerFromRoom(string playerId)
        {
            try
            {
                // Check if player is still in room
                var playerData = await FirebaseApi.GetRaw($"Rooms/{roomId}/Players/{playerId}");
                if (playerData == "null" || string.IsNullOrEmpty(playerData))
                {
                    // Player already left, just clean up heartbeat
                    await FirebaseApi.Delete($"Rooms/{roomId}/Heartbeat/{playerId}");
                    return;
                }

                // Delete player from room
                await FirebaseApi.Delete($"Rooms/{roomId}/Players/{playerId}");

                // Delete heartbeat
                await FirebaseApi.Delete($"Rooms/{roomId}/Heartbeat/{playerId}");

                // Decrease current players
                RoomClass.CurrentRoom.CurrentPlayers--;
                await FirebaseApi.Put($"Rooms/{roomId}/CurrentPlayers", RoomClass.CurrentRoom.CurrentPlayers);

                // Post leave event
                await RoomEvent.PostRoomEventAsync(roomId, "leave", playerId);

                GD.Print($"Player {playerId} removed due to disconnection");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error removing player {playerId}: {ex.Message}");
            }
        }

        private async Task HandleHostDisconnected()
        {
            try
            {
                GD.Print("Host disconnected, leaving room");

                // Leave room
                var room = RoomClass.CurrentRoom;
                if (room != null)
                {
                    await room.LeaveAsync();
                }

                // If no players left, delete room? But since host is gone, perhaps not necessary
                // For now, just leave
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error handling host disconnection: {ex.Message}");
            }
        }
    }
}