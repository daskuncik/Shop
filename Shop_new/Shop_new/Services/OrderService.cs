using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Shop_new.Models;

namespace Shop_new.Services
{
    public class OrderService : Service
    {
        public OrderService(IConfiguration conf) : base(conf.GetSection("Addresses")["Orders"]) { }

        public async Task<List<OrderModel>> GetOrdersForUser(int userid, int page, int perpage)
        {
            var httpResponseMessage = await Get($"{userid}?page={page}&perpage={perpage}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                var aa = JsonConvert.DeserializeObject<List<OrderModel>>(response);
                List<OrderModel> list = new List<OrderModel>();
                foreach (OrderModel el in aa)
                {
                    OrderModel model = new OrderModel();
                    model.Id = el.Id;
                    model.TotalSum = el.TotalSum;
                    model.Date = el.Date;
                    model.UserId = el.UserId;
                    list.Add(model);
                }
                return list;
                //return JsonConvert.DeserializeObject<List<string>>(response);
            }
            catch
            {
                return null;
            }
        }

        public async Task<OrderModel> GetOrder (int userid, int orderid)
        {
            var httpResponseMessage = await Get($"{userid}/order/{orderid}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                var aa = JsonConvert.DeserializeObject<OrderModel>(response);
                if (aa != null)
                    return aa;
                var model = new OrderModel()
                {
                    Id = 0,
                    UserId = 0,
                    TotalSum = 0
                };
                return model;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<OrderUnitModel>> GetUnitsForOrder(int userid, int orderid)
        {
            var httpResponseMessage = await Get($"{userid}/info/{orderid}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                if (response != null)
                {
                    if (response != "")
                    {
                        var aa = JsonConvert.DeserializeObject<List<OrderUnitModel>>(response);
                        List<OrderUnitModel> list = new List<OrderUnitModel>();
                        foreach (OrderUnitModel el in aa)
                        {
                            OrderUnitModel model = new OrderUnitModel();
                            model.GoodsId = el.GoodsId;
                            model.Count = el.Count;
                            model.OrderId = el.OrderId;
                            list.Add(model);
                        }
                        return list;
                    }
                    var ac = new List<OrderUnitModel>();
                    OrderUnitModel bb = new OrderUnitModel();
                    bb.GoodsId = 0;
                    bb.OrderId = 0;
                    bb.Count = 0;
                    ac.Add(bb);
                    return ac;
                    //return new List<OrderUnitModel>()
                }
                //else
                    return new List<OrderUnitModel>();
                //return JsonConvert.DeserializeObject<List<string>>(response);
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> CheckIfOrderExists(UserOrderModel model) => await PostJson("exists", model);
        //{
            //return await PostForm($"exists", new Dictionary<string, string> { { "orderid", model.orderId.ToString() }, { "userid", model.useridId.ToString() } });
        //}

        public async Task<HttpResponseMessage> AddOrder(int userid)
        {
            return await PostForm($"{userid}", new Dictionary<string, string> ());
        }

        public async Task<HttpResponseMessage> AddOrderUnit(int userid, int orderid, int goodsId)
        {
            return await PostForm($"{userid}/add/{orderid}", new Dictionary<string, string> { { "goodsid", goodsId.ToString() } });
        }

        public async Task<HttpResponseMessage> AddPrice(int userid, int orderid, int price)
        {
            return await PostForm($"{userid}/{orderid}", new Dictionary<string, string> { {"price", price.ToString() } });
        }

        public async Task<HttpResponseMessage> SubTotalSum(int userid, int orderid, int sum)
        {
            return await PostForm($"{userid}/{orderid}/sub", new Dictionary<string, string> { { "sum", sum.ToString() } });
        }

        public async Task<HttpResponseMessage> RemoveOrder(int userid, int orderId)
        {
            return await Delete($"{userid}/{orderId}");
        }

        public async Task<HttpResponseMessage> RemoveOrderUnit(int orderId, int goodsId)
        {
            return await Delete($"{orderId}/delete/{goodsId}");
        }


    }
}
