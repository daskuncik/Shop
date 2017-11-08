using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_new.Models
{
    public class OrderUnitModel
    {
        public int OrderId { get; set; }
        public int Count { get; set; }
        public int GoodsId { get; set; }
    }
}
