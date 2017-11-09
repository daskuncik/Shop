using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shop_new.Models;
using Shop_new.Services;
using Shop_new.Qeue;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Shop_new.Controllers
{
    public class MainController : Controller
    {
        private BillService billService;
        private OrderService orderService;
        private WareHouseService warehouseService;
        private ILogger<MainController> logger;

        public MainController(ILogger<MainController> logger,
            BillService _billService,
            OrderService _orderService,
            WareHouseService _warehouseService)
        {
            this.logger = logger;
            this.billService = _billService;
            this.orderService = _orderService;
            this.warehouseService = _warehouseService;
        }

        [HttpGet("{userid}/orders")] //для пользователя получить заказы
        public async Task<List<OrderModel>> GetOrders(int userid, int page = 0, int perpage = 0)
        {
            List<OrderModel> response = await orderService.GetOrdersForUser(userid, page, perpage);
            if (response != null)
            {
                logger.LogInformation($"Number of orders for user {userid}: {response.Count}");
               // return StatusCode(200, response);
                return response;
            }
            else
            {
                logger.LogCritical("Orders service unavailable");
                return null;
            }
        }

        [HttpGet("goods")] //получить все товары
        public async Task<List<WareHouseModel>> GetGoods(int page = 0, int perpage = 0)
        {
            List<WareHouseModel> response = await warehouseService.GetAllInfo(page, perpage);
            if (response != null)
            {
                //logger.LogInformation($"Number of orders for user {userid}: {response.Count}");
                return response;
            }
            else
            {
                logger.LogCritical("Goods service unavailable");
                return null;
            }
        }

        [HttpGet("bills")] //получить все счета, которые есть
        public async Task<List<string>> GetBills(int page = 0, int perpage = 0)
        {
            var response = (await billService.GetBills(page, perpage))?.Select(b => $"Billfor order {b.UserId} (pay {b.AmountPaid})").ToList();
            var response_2 = (await orderService.GetOrdersForUser(1, page, perpage))?.Select(b => $"Order: {b.Id}").ToList();
            if (response != null && response_2 != null)
            {
                //logger.LogInformation($"Number of orders for user {userid}: {response.Count}");
                return response.Concat(response_2).ToList();
            }
            if (response_2 != null)
                return response_2;

            if (response != null)
                return response;
           // else
            //{
                logger.LogCritical("Bills service unavailable");
                return null;
            //}
        }

        //считывает информацию с нескольких сервисов
        [HttpGet("{userid}/info/{orderid}")] //получить состав заказа
        public async Task<List<OrderUnitModel>> GetOrderUnitsForOrder(int userid, int orderid)
        {
            List<OrderUnitModel> response = await orderService.GetUnitsForOrder(userid, orderid);
            if (response != null)
            {
                //logger.LogInformation($"Number of orders for user {userid}: {response.Count}");
                return response;
            }
            else
            {
                logger.LogCritical("Orders service unavailable");
                return null;
            }
        }

        //модификация нескольких сервисов. откат
        [HttpPost("{userid}/addorder")] //добавить заказ
        public async Task<IActionResult> AddOrder(int userid)
        {
            var response = await orderService.AddOrder(userid);
            var aa = response.Content.ReadAsStringAsync().Result;
            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    int orderid = Convert.ToInt32(aa);
                    logger.LogInformation($"Order {orderid} successful created");
                    logger.LogInformation($"Create new bill");
                    var response_2 = await billService.AddBill(orderid);
                    if (response_2 != null)
                    {
                        if (response_2.IsSuccessStatusCode)
                        {
                            logger.LogInformation($"Bill for order {orderid} successful created");
                            return Ok();
                        }
                        //else
                        var response_3 = await orderService.RemoveOrder(userid, orderid);
                        return StatusCode(503, response.Content?.ReadAsStringAsync()?.Result);
                        //return BadRequest(response_2.Content?.ReadAsStringAsync()?.Result);
                    }
                    else
                    {
                        var response_3 = await orderService.RemoveOrder(userid, orderid);
                        if (response_3 != null)
                            return StatusCode(503, $"Bill servise unavailable. Rollback status for removig order: {(response_3.IsSuccessStatusCode ? "ok" : "failed")}");

                        return StatusCode(400, $"Bill servise unavailable. Order service unavailable: cann't remove created order"); 
                        // logger.LogCritical("Bills service unavailable");
                        //return NotFound("Service unavailable");
                    }
                }
                else
                    return StatusCode(503, response.Content?.ReadAsStringAsync()?.Result);
                    //return BadRequest(response.Content?.ReadAsStringAsync()?.Result);
            }
            else
            {
                logger.LogCritical("Orders service unavailable");
                return NotFound("Service unavailable");
            }
        }


        [HttpPost("{userid}/{orderid}/addorderunit/{goodsid}")] //добавить товар в заказ
        public async Task<IActionResult> AddOrderUnit(int userid, int orderid, int goodsid)
        {
            var response = await orderService.AddOrderUnit(userid, orderid, goodsid);
            if (response != null)
            {
                logger.LogInformation($"Attempt to add order unit, response {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var price = await warehouseService.GetGoodsPriceForId(goodsid);
                    if (price != null)
                    {
                       // logger.LogInformation($"Got price from warehouse, response {response.StatusCode}");
                        int price_sum = Convert.ToInt32(price.ToString());
                        var response_2 = await orderService.AddPrice(userid, orderid, price_sum);
                        if (response_2 != null)
                        {
                            logger.LogInformation($"Added sum to order, response {response_2.StatusCode}");
                            return Ok();
                        }
                        else
                        {
                            logger.LogCritical("Order service unavailable");
                            return StatusCode(503, "Order service unavailable");
                            //return NotFound("Service unavailable");
                        }
                    }
                    else
                    {
                        logger.LogCritical("Warehouse service unavailable");
                        return StatusCode(503, "Goods service unavailable");
                        //return NotFound("Service unavailable");
                    }
                }
                else
                    return BadRequest(response.Content?.ReadAsStringAsync()?.Result);
            }
            else
            {
                logger.LogCritical("Order service unavailable");
                return StatusCode(503, "Order service unavailable");
                //return NotFound("Service unavailable");
            }

        }

        //Читает 1 сервис, модифицирует другое
        [HttpPost("{userid}/addpayment")] //добавить платеж в заказ
        public async Task<IActionResult> AddPayment(int userid, int orderid, int sum)
        {

            var orderExists = await orderService.CheckIfOrderExists(new UserOrderModel { useridId = userid, orderId = orderid });
            logger.LogInformation($"Order response: {orderExists?.StatusCode}");
            if (orderExists == null)
                return StatusCode(503, "Order service unavailable");
            //return NotFound("Order service unavailable");
            if (orderExists.StatusCode != System.Net.HttpStatusCode.OK)
                return StatusCode(500, "Order doesn't exists");
            //return NotFound("Order doesn't exists");

            var response = await billService.GetBillForOrder(orderid);
            if (response != null)
            {
                if (response != "")
                {
                    var response_2 = await billService.AddPaymentToBill(orderid, sum);
                    if (response_2 != null)
                    {
                        if (response_2.IsSuccessStatusCode)
                            return Ok();
                        else
                            return BadRequest(response_2.Content?.ReadAsStringAsync()?.Result);
                    }
                    else
                    {
                        logger.LogCritical("Bill service unavailable");
                        return StatusCode(503, "Bill service unavailable");
                        //return NotFound("Service unavailable");
                    }
                }
                else
                {
                    logger.LogCritical($"Bill for order {orderid} not");
                    return StatusCode(500, $"Bill for order {orderid} not");
                    //return BadRequest();
                }
            }
            else
            {


                MyQeue.Retry(async () =>
                {
                using (var client = new HttpClient())
                {
                        var response_2 = await client.PostAsync($"http://localhost/{userid}/addpayment",
                            new FormUrlEncodedContent(new Dictionary<string, string> { { "orderid", orderid.ToString() }, {"sum", sum.ToString() } }));
                        if (response_2.IsSuccessStatusCode)
                            return true;
                        return false;
                    }
                });
                return StatusCode(503, "Services status: " +
                    $"Order: {(orderExists != null ? "online" : "offline")};" +
                    $"Bill: {(response != null ? "online" : "offline")};");
                //logger.LogCritical("Bill service unavailable");
                //return NotFound("Service unavailable");
            }
        }


        //с очередью
        [HttpDelete("{userid}/{orderid}/delete")] //удалить заказ
        public async Task<IActionResult> RemoveOrder(int userid, int orderid)
        {

            var response = await orderService.RemoveOrder(userid, orderid);
            if (response != null)
            {
                logger.LogInformation($"Attempt to remove order {orderid}, response {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var response_2 = await billService.RemoveBillByOrder(orderid);
                    if (response_2 != null && response_2.IsSuccessStatusCode)
                        return Ok();

                    MyQeue.Retry(async () =>
                    {
                        using (var client = new HttpClient())
                        {// try catch здесь
                            try
                            {
                                var response_3 = await billService.RemoveBillByOrder(orderid);
                                if (response_3 != null)
                                //var response_3 = await client.DeleteAsync($"http://localhost/{orderid}/deletebill");
                                //if (response_3.IsSuccessStatusCode)
                                return true;
                                return false;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    });
                    return StatusCode(503, "Services status: " +
                        $"Order: {(response != null ? "online" : "offline")};   " +
                        $"Bill: {(response_2 != null ? "online" : "offline")}; Status: {response_2?.StatusCode}");

                    //logger.LogCritical("Bill service unavailable");
                    //return StatusCode(503, "Bill service unavailable");
                    //return NotFound("Service unavailable");
                }
                return StatusCode(500, response.Content.ReadAsStringAsync()?.Result);
            }
            logger.LogCritical("Order service unavailable");
            return StatusCode(503, "Order service unavailable");
        }

        [HttpDelete("{orderid}/deletebill")] //удалить счет
        public async Task<IActionResult> RemoveBill( int orderid)
        {
            var response = await billService.RemoveBillByOrder(orderid);
            if (response != null && response.IsSuccessStatusCode)
                return Ok();
            return BadRequest();
        }

        //откат действий
        [HttpDelete("{userid}/{orderid}/deleteunit")] //удалить товар из заказа
        public async Task<IActionResult> RemoveOrderUnit(int userid, int orderid, int goodsid)
        {
            var response = await orderService.RemoveOrderUnit(orderid, goodsid);
            if (response != null)
            {
                logger.LogInformation($"Attempt to remove order {orderid}, response {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var price_resp = await warehouseService.GetGoodsPriceForId(goodsid);
                    if (price_resp != null)
                    {
                        if (price_resp != "")
                        {
                            var price = Convert.ToInt32(price_resp);
                            var subSum = await orderService.SubTotalSum(userid, orderid, price);
                            if (subSum != null && subSum.IsSuccessStatusCode)
                                return Ok();
                            else
                            {
                                var restore_order = await orderService.AddOrderUnit(userid, orderid, goodsid);
                                if (restore_order != null && restore_order.IsSuccessStatusCode)
                                    return StatusCode(503, $"Subtraction Error. Rollback order unit in order {orderid}");
                                return StatusCode(503, "Order service rollback error");
                            }

                        }
                        else
                            return StatusCode(503, $"Cann't find in warehouse goods with Id: {goodsid}");
                    }
                    else
                    {
                        var restore_order = await orderService.AddOrderUnit(userid, orderid, goodsid);
                        if (restore_order != null)
                            if (restore_order.IsSuccessStatusCode)
                                return StatusCode(503, $"Goods service unavailable. Rollback order unit in order {orderid}");
                    }
                            return Ok();

                }
                return BadRequest(response.Content.ReadAsStringAsync()?.Result);
            }
            logger.LogCritical("Order service unavailable");
            return NotFound("Service unavailable");
        }


        //[HttpGet("")]
        //public async Task<IActionResult> CreateObjectsInDB()
        //{
        //    logger.LogInformation($"Adding goods...");
        //    var response = await warehouseService.AddGoods("samsungTV", 15000, 12);
        //    if (response != null)
        //    {
        //        if (response.IsSuccessStatusCode)
        //            logger.LogInformation($"1. SamsungTv created");
        //        else
        //            logger.LogInformation($"1. SamsungTv error during creating");
        //    }
        //    else
        //    {
        //        logger.LogCritical("Goods service unavailable");
        //        return NotFound("Service unavailable");
        //    }
        //    ////////////
        //    var response_2 = await warehouseService.AddGoods("iPhone_8", 27000, 8);
        //    if (response != null)
        //    {
        //        if (response.IsSuccessStatusCode)
        //            logger.LogInformation($"2. iPhone_8 created");
        //        else
        //            logger.LogInformation($"2. iPhone_8 error during creating");
        //    }
        //    else
        //    {
        //        logger.LogCritical("Goods service unavailable");
        //        return NotFound("Service unavailable");
        //    }
        //    ////////////////
        //    var response_3 = await warehouseService.AddGoods("OpticalMouse", 300, 115);
        //    if (response != null)
        //    {
        //        if (response.IsSuccessStatusCode)
        //            logger.LogInformation($"3. OpticalMouse created");
        //        else
        //            logger.LogInformation($"3. OpticalMouse error during creating");
        //    }
        //    else
        //    {
        //        logger.LogCritical("Goods service unavailable");
        //        return NotFound("Service unavailable");
        //    }
        //    logger.LogInformation($"Goods added");
        //    return Ok();
        //}

    }
}
