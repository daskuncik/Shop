﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public class AddGoodsEvent : Event
    {
        public string Orderid { get; set; }
        public string Goodsid { get; set; }
    }
}