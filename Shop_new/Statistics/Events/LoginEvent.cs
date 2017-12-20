using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public class LoginEvent : Event
    {
        public string Name { get; set; }
        public string Origin { get; set; }
    }
}
