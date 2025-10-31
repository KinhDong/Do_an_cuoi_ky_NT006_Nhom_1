using System;
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
        public string Money { get; set; }
        public Image Avatar { get; set; }
        public bool IsHost { get; set; }

    }
}
