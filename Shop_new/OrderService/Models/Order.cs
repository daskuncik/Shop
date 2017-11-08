using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderService.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int TotalSum { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
    }
}