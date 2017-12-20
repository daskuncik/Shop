using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public class AckEvent : Event
    {
        public AckStatus Status { get; set; }
        public string Description { get; set; }
        public string AdjEventId { get; set; }
    }

    public enum AckStatus
    {
        Success,
        Failed
    }
}
