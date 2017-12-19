using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.Models
{
    public class Token
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public DateTime Expiration { get; set; }
    }
}
