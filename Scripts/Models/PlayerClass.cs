using Godot;

namespace NT106.Scripts.Models
{
    public class PlayerClass
    {
        public string Uid { get; set; }        
        public string InGameName { get; set; }
        public long Money { get; set; }
        public Texture2D Avatar;
        public int Seat;
        public string JoinedAt { get; set; }

        
    }    
}

