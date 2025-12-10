using Godot;
using NT106.Scripts.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NT106.Scripts.Models
{
    public class PlayerClass
    {
        public string Uid { get; set; }        
        public string InGameName { get; set; }
        public long Money { get; set; }
        public Texture2D Avatar { get; set; }
        public int Seat { get; set; }
        public string JoinedAt { get; set; }
        public List<(int, int)> Hands {get; set;}

        public async Task LoadAvatarAsync()
        {
            Avatar = await CloudinaryService.GetImageAsync(Uid);
        }
    }
}

