using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderService.Models
{
    public class OrderUnit
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public int GoodsId { get; set; }
        public int OrderId { get; set; }
    }
}