using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Statistics.Models;
using Statistics.Entities;

namespace Statistics.Controllers
{
    [Route("st")]
    public class MainStController : Controller
    {
        private AppDbContext db;

        public MainStController(AppDbContext dbContext)
        {
            this.db = dbContext;
        }

        [HttpGet("logins")]
        public async Task<List<LoginModel>> GetLogins()
        {
            //var result = db.Logins
            //    .Select(nai => nai.Username)
            //    .GroupBy(s => s)
            //    .Select(g => new LoginModel { Username = g.Key, Count = g.Count() })
            //    .ToList();


            var result = db.Logins
                .Select(q => new LoginModel
                {
                    username = q.Username,
                    time = q.DateTime
                }).ToList();

            return result;
        }

        [HttpGet("goods_additions")]
        public async Task<List<GoodsAddModel>> GetGoodsAdditions()
        {
            var result = db.GoodOperations
                .Where(q => q.Operation == Entities.Operation.Add)
                .Select(q => new GoodsAddModel {goodsId = Convert.ToInt32(q.goodsId),
                                                orderId = Convert.ToInt32(q.orderId),
                                                Time = q.actionMoment.ToString(@"ddMM-HH:mm")
                })
                .ToList();

            return result;
        }


        [HttpGet("goods_removings")]
        public async Task<List<GoodsAddModel>> GetGoodsRemovings()
        {
            var result = db.GoodOperations
                .Where(q => q.Operation == Entities.Operation.Delete)
                .Select(q => new GoodsAddModel
                {
                    goodsId = Convert.ToInt32(q.goodsId),
                    orderId = Convert.ToInt32(q.orderId),
                    Time = q.actionMoment.ToString(@"ddMM-HH:mm")
                })
                .ToList();

            return result;
        }

        [HttpGet("requests")]
        public async Task<List<RequestModel>> GetRequests()
        {
            int forSeconds = 60;
            var requests = db.Requests.Where(i => i.RequestType == Events.RequestType.Shop_new).ToList();
            var minDate = DateTime.Now - TimeSpan.FromSeconds(forSeconds);
            var maxDate = DateTime.Now;
            int numOfIntervals = 10;
            var span = (maxDate - minDate) / numOfIntervals;
            var intervaledRequests = new Dictionary<DateTime, List<RequestInfo>>();
            for (int i = 0; i < numOfIntervals; i++)
            {
                intervaledRequests.Add(minDate + 0.5 * span + span * i, new List<RequestInfo> { new RequestInfo { Time = minDate + 0.5 * span + span * i, From = null } });
            }
            foreach (var request in requests)
            {
                foreach (var key in intervaledRequests.Keys)
                    if ((key - request.Time).Duration() < (0.5 * span))
                    {
                        intervaledRequests[key].Add(request);
                        break;
                    }
            }
            var test = intervaledRequests.Values.Select(v => v.Count()).ToList();
            var diffconst = 0.5 * (forSeconds / numOfIntervals);
            return intervaledRequests
                .SelectMany(kv => kv.Value.GroupBy(i => i.From).Select(g => (kv.Key, g.Key, g.ToList())))
                .Select(t => new RequestModel
                {
                    count = t.Item2 == null ? 0 : t.Item3.Count,
                    from = t.Item2,
                    time = $"{((t.Item1 - maxDate).Seconds - diffconst).ToString()}s - {((t.Item1 - maxDate).Seconds + diffconst).ToString()}s"
                }).ToList();
            //var result = db.Requests
            //    .Select(q => new RequestModel
            //    {
            //        From = q.From,
            //        Time = q.Time.ToString(),
            //        To = q.To
            //    })
            //    .ToList();

            //return result;

        }

        [HttpGet("requests/detail")]
        public async Task<List<RequestDetailModel>> GetRequestDetails()
        {
            return db.Requests.Select(i => new RequestDetailModel { from = i.From, time = i.Time, to = i.To }).ToList();
        }

        [HttpGet("orders_additions/detail")]
        public async Task<List<OrderDetailModel>> GetOrdersAdditionsDetail()
        {
            var result = db.OrderOperations
                .Where(q => q.Operation == Entities.Operation.Add)
                .Select(q => new OrderDetailModel
                {
                    userId = Convert.ToInt32(q.userId),
                    orderId = Convert.ToInt32(q.orderId),
                    sum = 0,
                    time = q.actionMoment.ToString(@"ddMM-HH:mm")
                })
                .ToList();
            result.GroupBy(x => new { x.userId });
            return result;
        }

        [HttpGet("orders_additions")]
        public async Task<List<OrderModel>> GetOrdersAdditions()
        {
            var result = db.OrderOperations
                .Where(q => q.Operation == Entities.Operation.Add)
                .Select (q => q.userId)
                .GroupBy (s => s)
                .Select(q => new OrderModel
                {
                    username = q.Key,
                    count = q.Count()
                })
                .ToList();
            return result;
        }

        [HttpGet("orders_removings")]
        public async Task<List<OrderDetailModel>> GetOrdersRemoving()
        {
            var result = db.OrderOperations
                .Where(q => q.Operation == Entities.Operation.Delete)
                .Select(q => new OrderDetailModel
                {
                    userId = Convert.ToInt32(q.userId),
                    orderId = Convert.ToInt32(q.orderId),
                    sum = 0,
                    time = q.actionMoment.ToString(@"ddMM-HH:mm")
                })
                .ToList();

            return result;
        }

        [HttpGet("orders_add_pay")]
        public async Task<List<OrderDetailModel>> GetOrdersPay()
        {
            var result = db.OrderOperations
                .Where(q => q.Operation == Entities.Operation.Add_Pay)
                .Select(q => new OrderDetailModel
                {
                    userId = Convert.ToInt32(q.userId),
                    orderId = Convert.ToInt32(q.orderId),
                    sum = Convert.ToInt32(q.sum),
                    time = q.actionMoment.ToString(@"ddMM-HH:mm")
                })
                .ToList();

            return result;
        }

    }
}
