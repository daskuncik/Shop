using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Entities
{
    public class GoodsOperationInfo
    {
        public string Id { get; set; }
        public string orderId { get; set; }
        public string goodsId { get; set; }
        public Operation Operation { get; set; }
        public DateTime actionMoment { get; set; }
    }
}
