using Godot;
using System.Collections.Generic;

namespace NT106.Scripts.Models
{
    public class PlayerClass
    {
        public string Uid { get; set; }        
        public string InGameName { get; set; }
        public long Money { get; set; }
        public int Seat;
        public string JoinedAt { get; set; }

        public List<CardClass> Cards { get; set; } = new List<CardClass>();
    }    
}

