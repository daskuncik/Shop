using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Entities
{
    public class OrderOperationInfo
    {
        public string Id { get; set; }
        public string orderId { get; set; }
        public string userId { get; set; }
        public string sum { get; set; }
        public Operation Operation { get; set; }
        public DateTime actionMoment { get; set; }
    }

    public enum Operation
    {
        Add,
        Add_Pay,
        Delete
    }
}
