using Godot;
using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NT106.Scripts.Services;

public class PlayerClass
{
    public string Uid { get; set; }
    public string InGameName { get; set; }
    public bool IsHost { get; set; }
    public long Money { get; set; }
    public string JoinedAt { get; set; }
}