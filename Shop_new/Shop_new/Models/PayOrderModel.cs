﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_new.Models
{
    public class PayOrderModel
    {
        public int Id { get; set; }
        public int TotalSum { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public int AmountPaid { get; set; }
    }
}
