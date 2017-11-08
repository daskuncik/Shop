using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WareHouseService.Models;
using Microsoft.Extensions.Logging;
using Shop_new.Models;
using Newtonsoft.Json;

namespace WareHouseService.Controllers
{
    [Route("api")]
    public class GoodsController : Controller
    {
        GoodsDbContext db = new GoodsDbContext();
        private ILogger<GoodsController> logger;

        public GoodsController(GoodsDbContext db, ILogger<GoodsController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        //сделать функцию получения цены за товар
        //получить подробную информацию по всем товарам
        [HttpGet("{name}")]
        public async Task<List<string>> GetInfForGoodName([FromRoute]string name, int page, int perpage)
        {
            logger.LogDebug($"Searching goods with name {name}");
            var good = db.Goods.Where(q => q.Name == name);
            if (good != null && good.Count() > 0)
            {
                logger.LogDebug($"Goods with name {name}: {good.Count()}");
                if (perpage != 0 && page != 0)
                {
                    logger.LogDebug($"Skipping {perpage * page} goods due to pagination");
                    good = good.Skip(perpage * page);
                }
                if (perpage != 0)
                {
                    logger.LogDebug($"Retrieving at max {perpage} goods");
                    good = good.Take(perpage);
                }
                return good.Select(g => $"Name: {g.Name}{Environment.NewLine}Price: {g.Price}{Environment.NewLine}Count: {g.Count}").ToList();
            }
            logger.LogWarning($"No goods with this name: {name}");
            return new List<string>();
        }

        [HttpGet("goods")]
        public async Task<string> GetInfo (int page, int perpage)
        {
            logger.LogDebug($"Searching goods ");
            var good = db.Goods.Select(q => q) ;
                if (perpage != 0 && page != 0)
                {
                    logger.LogDebug($"Skipping {perpage * page} goods due to pagination");
                    good = good.Skip(perpage * page);
                }
                if (perpage != 0)
                {
                    logger.LogDebug($"Retrieving at max {perpage} goods");
                    good = good.Take(perpage);
                }
            // var list = new List<WareHouseModel>();
            List<Good> resList = new List<Good>();
            foreach (var el in good)
            {
                resList.Add(new Good { Name=el.Name, Count = el.Count, Price=el.Price});
            }
            return JsonConvert.SerializeObject(resList);
            //return good.Select(g => $"Name: {g.Name}{Environment.NewLine}Price: {g.Price}{Environment.NewLine}Count: {g.Count}").ToList();

        }


        //получить информацию по конкретному товару (ID)
        [HttpGet("{id}/byid")]
        public async Task<string> GetGoodsForId([FromRoute]int id)
        {
            logger.LogDebug($"Searching goods with id: {id}");
            var good = db.Goods.FirstOrDefault(q => q.Id == id);
            if (good != null)
            {
                WareHouseModel model = new WareHouseModel { Name = good.Name, Count = good.Count, Price = good.Price };
                //string str = "";
               // str += $"Name: {good.Name}{Environment.NewLine}";
                //str += $"Price: {good.Price}{Environment.NewLine}";
               // str += $"Count: {good.Count}{Environment.NewLine}";
                logger.LogDebug($"Returning goods with id: {id}");
                return JsonConvert.SerializeObject(model);

            }
            logger.LogDebug($"Goods with id: {id} not found");
            return "";
        }

        //получить информацию по конкретному товару (NAME)
        [HttpGet("{name}/byname")]
        public async Task<string> GetGoodsForName([FromRoute]string name)
        {
            logger.LogDebug($"Searching goods with name: {name}");
            var good = db.Goods.FirstOrDefault(q => q.Name == name);
            if (good != null)
            {
                string str = "";
                str += $"Name: {good.Name}{Environment.NewLine}";
                str += $"Price: {good.Price}{Environment.NewLine}";
                str += $"Count: {good.Count}{Environment.NewLine}";
                logger.LogDebug($"Returning goods with name: {name}");
                return str;
            }
            logger.LogDebug($"Goods with name: {name} not found");
            return "";
        }

        //получить толькоцену за товар
        [HttpGet("{id}/price")]
        public async Task<string> GetGoodsPriceForId([FromRoute]int id)
        {
            logger.LogDebug($"Searching goods with id: {id}");
            var good = db.Goods.FirstOrDefault(q => q.Id == id);
            if (good != null)
            {
                logger.LogDebug($"Returning goods price");
                return good.Price.ToString();
            }
            logger.LogDebug($"Goods with id: {id} not found");
            return "";
        }

        //добавить товар
        [HttpPost("")]
        public async Task<IActionResult> AddGoods(string name, int price, int count)
        {
            var gg = db.Goods.FirstOrDefault(q => q.Name == name && q.Price == price);
            if (gg == null)
            {
                logger.LogDebug($"Adding goods: {name}");
                var g_new = new Good { Name = name, Price = price, Count = count };
                var state = db.Goods.Add(g_new)?.State;
                logger.LogDebug($"Goods {name} added with state {state}");
                db.SaveChanges();
                return Ok();
            }
            else
            {
                logger.LogDebug($"Goods {name} already exists. Incremented count of its.");
                gg.Count++;
                db.SaveChanges();
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveGoods([FromRoute]int id)
        {
            logger.LogDebug($"Removing goods {id}");
            var goods = db.Goods.FirstOrDefault(q => q.Id == id);
            if (goods != null)
            {
                logger.LogDebug($"Goods with id {id} exists in database");
                var state = db.Goods.Remove(goods)?.State;
                logger.LogDebug($"Goods {id} removed with state {state}");
                db.SaveChanges();
                return Ok();
            }
            else
            {
                logger.LogWarning($"Goods {id} not found");
                return BadRequest();
            }
        }
    }
}
