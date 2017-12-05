using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionManager.Entities
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Subscriber { get; set; }
        public string Author { get; set; }
    }
}
