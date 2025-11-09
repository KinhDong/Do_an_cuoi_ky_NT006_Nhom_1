using System;
using Newtonsoft.Json;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NT106.Models
{
    public class PlayerClass
    {
        public string Uid { get; set; }
        public string InGameName { get; set; }
        public long Money { get; set; }
        [JsonIgnore]
        public Image Avatar { get; set; }
        public bool IsHost { get; set; }        
        public string JoinedAt { get; set; }
    }
}
