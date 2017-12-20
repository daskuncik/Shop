using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public class DeleteOrderEvent : Event
    {
        public string Orderid { get; set; }
        public string Userid { get; set; }
    }
}
