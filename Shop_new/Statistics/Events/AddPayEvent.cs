using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public class AddPayEvent : Event
    {
        public string Userid { get; set; }
        public string Orderid { get; set; }
        public string Sum { get; set; }
    }
}
