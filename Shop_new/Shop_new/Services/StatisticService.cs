using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_new.Services
{
    public class StatisticService : Service
    {
        public StatisticService(IConfiguration conf) : base(conf.GetSection("Addresses")["Stat"]) { }

        public async Task<List<GoodsAddModel>> GetGoodsAdditions()
        {
            var res = await Get("goods_additions");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<GoodsAddModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<GoodsAddModel>> GetGoodsRemovings()
        {
            var res = await Get("goods_removings");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<GoodsAddModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<RequestModel>> GetRequests()
        {
            var res = await Get("requests");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<RequestModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<RequestDetailModel>> GetRequestsDetailed()
        {
            var res = await Get("requests/detail");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<RequestDetailModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<OrderModel>> GetOrdersAdditions()
        {
            var res = await Get("orders_additions");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<OrderModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<OrderDetailModel>> GetOrdersAdditionsDetail()
        {
            var res = await Get("orders_additions/detail");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<OrderDetailModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<OrderDetailModel>> GetOrdersRemovings()
        {
            var res = await Get("orders_removings");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<OrderDetailModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<OrderDetailModel>> GetOrdersPay()
        {
            var res = await Get("orders_add_pay");
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<List<OrderDetailModel>>(res.Content.ReadAsStringAsync().Result);
            return null;
        }

        public async Task<List<LoginModel>> GetLoginDetailed()
        {
            var res = await Get("logins");
            if (res.IsSuccessStatusCode)
            {
                var b = JsonConvert.DeserializeObject<List<LoginModel>>(res.Content.ReadAsStringAsync().Result);
                return b;
            }
            return null;
        }
    }
}
