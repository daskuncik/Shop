using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public abstract class Event
    {
        public DateTime OccurenceTime { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
