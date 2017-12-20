using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public class AddUserEvent : Event
    {
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
