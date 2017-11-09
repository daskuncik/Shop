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
    public class BillService : Service
    {
        public BillService(IConfiguration conf) : base(conf.GetSection("Addresses")["Bills"]) { }


        public async Task<string> GetBillForOrder(int orderid)
        {
            var httpResponseMessage = await Get($"{orderid}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<string>(response);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<BillModel>> GetBills(int page, int perpage)
        {
            var httpResponseMessage = await Get($"bills?page={page}&perpage={perpage}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                var aa = JsonConvert.DeserializeObject<List<BillModel>>(response);
                List<BillModel> list = new List<BillModel>();
                foreach (BillModel el in aa)
                {
                    BillModel model = new BillModel();
                    model.UserId = el.UserId;
                    model.AmountPaid = el.AmountPaid;
                    list.Add(model);
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> AddBill(int orderid)
        {
            try
            {
                return await PostForm($"{orderid}", new Dictionary<string, string>());
            }
            catch
            {
                return null;
            }
        }


        public async Task<HttpResponseMessage> AddPaymentToBill(int orderid, int sum)
        {
            try
            {
                return await PostForm($"{orderid}/pay", new Dictionary<string, string>() { { "sum", sum.ToString() } });
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> RemoveBill(int id)
        {
            try
            {
                return await Delete($"{id}/delete");
            }
            catch
            {
                return null;
            }
        }
        public async Task<HttpResponseMessage> RemoveBillByOrder(int id)
        {
            try
            {
                return await Delete($"{id}/byorder");
            }
            catch
            {
                return null;
            }
        }

    }
}
