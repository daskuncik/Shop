using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Models
{
    public class OrderDetailModel
    {
        public int orderId { get; set; }
        public int userId { get; set; }
        public int sum { get; set; }
        public string time { get; set; }
    }
}
