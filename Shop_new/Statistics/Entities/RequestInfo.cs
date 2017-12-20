using Statistics.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Entities
{
    public class RequestInfo
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Time { get; set; }
        public RequestType RequestType { get; set; }
    }
}
