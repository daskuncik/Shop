using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_new.Models
{
    public class Token
    {
        public int Id { get; set; }
        public string token { get; set; }
        public string owner { get; set; }
        public DateTime Item1 { get; set; }
        public DateTime Item2 { get; set; }
    }
}
