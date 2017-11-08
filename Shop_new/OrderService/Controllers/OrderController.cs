using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shop_new.Models;

namespace OrderService.Controllers
{
    [Route("api")]
    public class OrderController : Controller
    {
        private OrderDbContext db;
        private ILogger<OrderController> logger;

        public OrderController(OrderDbContext db, ILogger<OrderController> logger)
        {
            this.db = db;
            this.logger = logger;
            this.logger = logger;
        }


        [HttpGet("{userid}")]
        public async Task<string> GetOrdersForUser(int userid, int page, int perpage)
        {
            logger.LogDebug($"Retriving all orders for user {userid}");
            var result = db.Orders.Where(s => s.UserId == userid);
            logger.LogDebug($"Found {result.Count()} orders for user {userid}");
            if (page != 0 && perpage != 0)
            {
                logger.LogDebug($"Skipping {page * perpage} entities due to pagination");
                result = result.Skip(page * perpage);
            }
            if (perpage != 0)
            {
                logger.LogDebug($"Taking at max {perpage} entities");
                result = result.Take(perpage);
            }
            logger.LogDebug($"Retrieved {result.Count()} orders: {string.Join(", ", result)}");
            List<Order> resList = new List<Order>();
            foreach (var el in result)
            {
                resList.Add(new Order { Id = el.Id, Date = el.Date, TotalSum = el.TotalSum, UserId = el.UserId }); 
            }
            return JsonConvert.SerializeObject(resList);
        }

        [HttpGet("{userid}/info/{orderid}")]
        public async Task<string> GetOrderInfoForUser(int userid, int orderid)
        {
            logger.LogDebug($"Retriving one order for user {userid}");
            var result = db.Orders.FirstOrDefault(q => q.Id == orderid && q.UserId == userid);
            if (result != null)
            {
                logger.LogDebug($"Found order for user {userid}");
                int count = db.OrderUnits.Where(q => q.OrderId == orderid).Count();
                if (count > 0)
                {
                    var units = db.OrderUnits.Where(q => q.OrderId == orderid);
                    List<OrderUnit> list = new List<OrderUnit>();
                    foreach(OrderUnit el in units)
                    {
                        list.Add(new OrderUnit { OrderId = el.OrderId, GoodsId = el.GoodsId, Count = el.Count });
                    }
                    return JsonConvert.SerializeObject(list);
                }
                return "No units";
            }
            else
            {
                logger.LogDebug($" bill for order {orderid} not found");
                return "";
            }
        }

        [HttpPost("exists")]
        public async Task<IActionResult> CheckIfOrderExists(/*[FromBody] int orderid, int userid) */[FromBody] UserOrderModel model)
        {
            int orderid = model.orderId;
            int userid = model.useridId;
            logger.LogDebug($"Order request, orderid: {orderid} for user {userid}");
            var ord = db.Orders.FirstOrDefault(q => q.Id == orderid && q.UserId == userid);
            if (ord != null)
            {
                logger.LogDebug($"Order {orderid} found");
                return Ok();
            }
            logger.LogWarning($"Order {orderid} not found");
            return BadRequest();
        }


        [HttpPost("{userid}")]
        public async Task<int> AddOrder(int userid)
        {
            //var prevSubscription = db.Subscriptions.FirstOrDefault(s => s.Subscriber == subscriber.ToLowerInvariant() && s.Author == author.ToLowerInvariant());
            //if (orderForDeleting == null)
            //{
            logger.LogDebug($"Adding order for user: {userid}");
            var newOrder = new Order { TotalSum = 0, UserId = userid, Date = DateTime.Now };
            var state = db.Orders.Add(newOrder)?.State;
                //var state = db.Subscriptions.Add(subscription)?.State;
            logger.LogDebug($"New order for user {userid} added with state {state}");
            db.SaveChanges();
            var order = db.Orders.Last(q => q.UserId == userid);
            return order.Id;
            //return Ok();
            //}
        }


        [HttpPost("{userid}/add/{orderid}")]
        public async Task<IActionResult> AddOrderUnit(int userid, int orderId, int goodsId)
        {
            var order = db.Orders.FirstOrDefault(q => q.Id == orderId && q.UserId == userid);
            //var prevSubscription = db.Subscriptions.FirstOrDefault(s => s.Subscriber == subscriber.ToLowerInvariant() && s.Author == author.ToLowerInvariant());
            if (order == null)
            {
                logger.LogWarning($"Order {orderId} for user {userid} doesn't exists");
                return BadRequest();
            }
            else
            {
                var orderUnit = db.OrderUnits.FirstOrDefault(q => q.GoodsId == goodsId && q.OrderId == orderId);
                if (orderUnit == null)
                {
                    logger.LogDebug($"Creating new order unit for order: {orderId}");
                    var newOrderUnit = new OrderUnit { Count = 1, OrderId = orderId, GoodsId = goodsId };
                    var state = db.OrderUnits.Add(newOrderUnit)?.State;
                    logger.LogDebug($"New order unit for order {orderId} added with state {state}");
                    db.SaveChanges();
                }
                else
                {
                    logger.LogDebug($"Increment count order unit for order unit: {orderUnit.Id}");
                    db.Entry(orderUnit).State = EntityState.Modified;
                    orderUnit.Count++;
                    db.SaveChanges();
                }     
            }
            return Ok();
        }

        //добавление цены в общую сумму заказа
        [HttpPost("{userid}/{orderid}")]
        public async Task<IActionResult> AddPriceToOrder(int userid, int orderId, int price)
        {
            var order = db.Orders.FirstOrDefault(q => q.Id == orderId && q.UserId == userid);
            if (order != null)
            {
                order.TotalSum += price;
                db.SaveChanges();
                logger.LogDebug($"Added price={price} to order {orderId}");
                return Ok();
            }
            else
            {
                logger.LogWarning($"Order {orderId} for user {userid} not found");
                return BadRequest();
            }
        }

        //вычесть цену удаленного товара из заказа
        [HttpPost("{userid}/{orderid}/sub")]
        public async Task<IActionResult> SubFromTotalSum (int userid, int orderid, int sum)
        {
            var order = db.Orders.FirstOrDefault(q => q.Id == orderid && q.UserId == userid);
            if (order != null)
            {
                order.TotalSum -= sum;
                db.SaveChanges();
                return Ok();
            }
            return null;
        }


        [HttpDelete("{userid}/{orderid}")]
        public async Task<IActionResult> RemoveOrder(int userid, int orderid)
        {
            logger.LogDebug($"Removing order {orderid} for user {userid}");
            var order = db.Orders.FirstOrDefault(q => q.Id == orderid && q.UserId == userid);
            if (order != null)
           {
                logger.LogDebug($"Order {orderid} for user {userid} exists in database");
                var orderUnits = db.OrderUnits.Where(q => q.OrderId == orderid);
                if (orderUnits != null)
                {
                    foreach (var unit in orderUnits)
                    {
                        db.OrderUnits.Remove(unit);
                    }
                    logger.LogDebug($" All order units of order {orderid} removed");
                    db.SaveChanges();
                }
                var state = db.Orders.Remove(order)?.State;
                logger.LogDebug($"Order {orderid} for user {userid} removed with state {state}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"Order {orderid} for user {userid} not found");
            return BadRequest();
        }


        [HttpDelete("{orderid}/delete/{goodsid}")]
        public async Task<IActionResult> RemoveOrderUnit(int orderid, int goodsid)
        {
            logger.LogDebug($"Removing order unit {goodsid} for order {orderid}");
            var orderunit = db.OrderUnits.FirstOrDefault(q => q.OrderId == orderid && q.GoodsId == goodsid);
            if (orderunit != null)
            {
                logger.LogDebug($"Removing order unit {goodsid} for order {orderid}");
                var state = db.OrderUnits.Remove(orderunit)?.State;
                logger.LogDebug($"Order unit {goodsid} removed from order {orderid} with state {state}");
                db.SaveChanges();
                return Ok();
            }
            else
            {
                logger.LogWarning($"Order unit {goodsid} for order {orderid} not found");
                return BadRequest();
            }
        }

        //удалить из всех заказов указанную вещь 
        [HttpDelete("{goodsid}")]
        public async Task<IActionResult> RemoveOrderUnitForGoodsId(int goodsid)
        {
            logger.LogDebug($"Removing order unit {goodsid}");
            var orderunit = db.OrderUnits.FirstOrDefault(q => q.GoodsId == goodsid);
            if (orderunit != null)
            {
                logger.LogDebug($"Removing order unit {goodsid} ");
                var state = db.OrderUnits.Remove(orderunit)?.State;
                logger.LogDebug($"Order unit {goodsid} removed  with state {state}");
                db.SaveChanges();
                return Ok();
            }
            else
            {
                logger.LogWarning($"Order unit {goodsid} not found");
                return BadRequest();
            }
        }
    }
}
