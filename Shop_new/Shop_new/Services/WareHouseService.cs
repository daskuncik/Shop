using Shop_new.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shop_new.Services
{
    public class WareHouseService : Service
    {
        public WareHouseService(IConfiguration conf)
            : base(conf.GetSection("Addresses")["Goods"]) { }


        public async Task<List<WareHouseModel>> GetGoods(string name, int page, int perpage)
        {
            var httpResponseMessage = await Get($"{name.ToLowerInvariant()}?page={page}&perpage={perpage}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                var aa = JsonConvert.DeserializeObject<List<dynamic>>(response);
                List<WareHouseModel> list = new List<WareHouseModel>();
                foreach (WareHouseModel el in aa)
                {
                    WareHouseModel model = new WareHouseModel();
                    model.Count = el.Count;
                    model.Name = el.Name;
                    model.Price = el.Price;
                    model.Id = el.Id;
                    list.Add(model);
                }
                return list;
                //return JsonConvert.DeserializeObject<List<WareHouseModel>>(response);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<WareHouseModel>> GetAllInfo( int page, int perpage)
        {
            var httpResponseMessage = await Get($"goods/?page={page}&perpage={perpage}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                var aa = JsonConvert.DeserializeObject<List<WareHouseModel>>(response);
                List<WareHouseModel> list = new List<WareHouseModel>();
                foreach (WareHouseModel el in aa)
                {
                    WareHouseModel model = new WareHouseModel();
                    model.Count = el.Count;
                    model.Name = el.Name;
                    model.Price = el.Price;
                    model.Id = el.Id;
                    list.Add(model);
                }
                return list;
                //return JsonConvert.DeserializeObject<List<WareHouseModel>>(response);
            }
            catch
            {
                return null;
            }
        }

        ///////  !!!! преобразовать возвращаемое значение в строку ,если не пойдет   !!!!!!!!!!!!
        public async Task<WareHouseModel> GetGoodsForId(int id)
        {
            var httpResponseMessage = await Get($"{id}/byid");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                dynamic aa = JsonConvert.DeserializeObject(response);
                WareHouseModel model = new WareHouseModel();
                model.Count = aa.Count;
                model.Name = aa.Name;
                model.Price = aa.Price;
                model.Id = aa.Id;
                return model;
                //return JsonConvert.DeserializeObject<WareHouseModel>(response);
            }
            catch
            {
                return null;
            }
        }


        public async Task<WareHouseModel> GetGoodsForName(string name)
        {
            var httpResponseMessage = await Get($"{name}/byname");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                dynamic aa = JsonConvert.DeserializeObject(response);
                WareHouseModel model = new WareHouseModel();
                model.Count = aa.Count;
                model.Name = aa.Name;
                model.Price = aa.Price;
                model.Id = aa.Id;
                return model;
                //return JsonConvert.DeserializeObject<WareHouseModel>(response);
            }
            catch
            {
                return null;
            }
        }

        //получить толькоцену за товар
        public async Task<string> GetGoodsPriceForId(int id)
        {
            var httpResponseMessage = await Get($"{id}/price");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            return response;
        }

        //добавить товар
        public async Task<HttpResponseMessage> AddGoods(string name, int price, int count)
        {
            var obj = new { Name = name, Price = price, Count = count };
            return await PostForm("", new Dictionary<string, string>{ { "name", name }, { "price", price.ToString() }, { "count", count.ToString() } });
        }


        public async Task<HttpResponseMessage> RemoveOrder(int id)
        {
            return await Delete($"{id}");
        }
    }
}
