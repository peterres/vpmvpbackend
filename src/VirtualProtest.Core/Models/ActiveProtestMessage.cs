using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualProtest.Core.Models
{
    public class ActiveProtestMessage
    {
        public string? ProtestId { get; set; }

        public int ParticipantCountAll { get; set; }

        public int ParticipantCountActive { get; set; }
    }
}
