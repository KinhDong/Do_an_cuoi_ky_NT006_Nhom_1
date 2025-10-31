using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NT106.Models
{
    public class RoomClass
    {
        public string RoomId { get; set; }
        public string HostUid { get; set; }
        public List<PlayerClass> playerClasses { get; set; }
        public string Status { get; set; }
    }
}
