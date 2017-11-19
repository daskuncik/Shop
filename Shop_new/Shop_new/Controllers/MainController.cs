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
using Newtonsoft.Json;

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
        public async Task<IActionResult> GetOrders(int? userid, int? page, int? perpage)
        {
            ViewData["Title"] = "Orders";
            if (userid == null)
            {
                ViewBag.UserId = null;
                ViewBag.Message = "Invalid User";
                return PartialView("_GetOrders");
                //return StatusCode(400, "Invalid user"); //null;
            }
            if ((page != null && perpage == null) || (page == null && perpage != null))
            {
                ViewBag.UserId = userid.GetValueOrDefault(); ;
                ViewBag.Message = "Invalid Pages";
                return PartialView("_GetOrders");
                //return StatusCode(400, "Invalid pages");//null;
            }
            

            if (page == null && perpage == null)
            {
                page = 0;
                perpage = 0;
            }
            List<OrderModel> response = await orderService.GetOrdersForUser(userid.GetValueOrDefault(), page.GetValueOrDefault(), perpage.GetValueOrDefault());
            if (response != null)
            {
                List<PayOrderModel> resultList = new List<PayOrderModel>();
                foreach (var order in response)
                {
                    string response_bill = await billService.GetBillForOrder(order.Id);
                    if (response_bill != "")
                    {
                        var model = new PayOrderModel
                        {
                            Id = order.Id,
                            Date = order.Date,
                            TotalSum = order.TotalSum,
                            UserId = order.UserId,
                            AmountPaid = Convert.ToInt32(response_bill)
                        };
                        resultList.Add(model);
                    }
                }
                ViewBag.UserId = userid;
                ViewBag.Orders = resultList;
                logger.LogInformation($"Number of orders for user {userid.GetValueOrDefault()}: {response.Count}");
                // return StatusCode(200, response);
                //return response;
                //return StatusCode(200, response); //JsonConvert.SerializeObject(response);
                return View(resultList);
            }
            else
            {
                logger.LogCritical("Orders service unavailable");
                ViewBag.UserId = userid.GetValueOrDefault();
                ViewBag.Message = "Orders servise unavailable";
                return PartialView("_GetOrders");
                //return View();
            }
        }

        [HttpGet("{userid}/goods")] //получить все товары
        public async Task<IActionResult> GetGoods(int? userid, int page = 0, int perpage = 0)
        {
            ViewBag.UserId = userid;
            List<OrderModel> response_order = await orderService.GetOrdersForUser(userid.GetValueOrDefault(), 0, 0);
            if (response_order.Count() > 0)
                ViewBag.OrderIds = response_order;
            else
                ViewBag.OrderIds = null;
            List<WareHouseModel> response = await warehouseService.GetAllInfo(page, perpage);
            if (response != null)
            {
                //logger.LogInformation($"Number of orders for user {userid}: {response.Count}");
                return View(response);
                //return response;
            }
            else
            {
                logger.LogCritical("Goods service unavailable");
                ViewBag.Message = "Goods service unavailable";
                return PartialView("_GetGoods");
                //return null;
            }
        }

        //деградация функционала
        [HttpGet("bills")] //получить все счета и заказы, которые есть
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


        [HttpGet("{userid}/info/{orderid}")] //получить состав заказа
        public async Task<IActionResult> GetOrderUnitsForOrder(int? userid, int? orderid)
        {
            
            if (userid == null)
            {
                ViewBag.Message = "Invalid User";
                return PartialView("_GetOrderUnitsForOrder");
                //return StatusCode(400, "Invalid user");
            }
            if (orderid == null)
            {
                //return StatusCode(400, "Invalid order");
                ViewBag.Message = "Invalid order";
                return PartialView("_GetOrderUnitsForOrder");
            }
            ViewBag.UserId = userid.GetValueOrDefault();
            var response1 = await orderService.GetOrder(userid.GetValueOrDefault(), orderid.GetValueOrDefault());
            if (response1 != null)
            {
                ViewBag.Date = response1.Date;
                ViewBag.OrderId = response1.Id;
                ViewBag.TotalSum = response1.TotalSum;
                List<OrderUnitModel> response = await orderService.GetUnitsForOrder(userid.GetValueOrDefault(), orderid.GetValueOrDefault());
                if (response != null)
                {
                    //logger.LogInformation($"Number of orders for user {userid}: {response.Count}");
                    //return response;
                    if (response.Count() > 0)
                    {
                        if (response.Count() == 1 && response[0].Count == 0 && response[0].GoodsId == 0 && response[0].OrderId == 0)
                        {
                            ViewBag.Message = $"Order {orderid} not found";
                            return PartialView("_GetOrderUnitsForOrder");
                        }
                        var unitlist = new List<GoodsUnitModel>();
                        foreach (var unit in response)
                        {
                            var goodsunit = new GoodsUnitModel
                            {
                                Count = unit.Count,
                                GoodsId = unit.GoodsId
                            };
                            var goods = await warehouseService.GetGoodsForId(unit.GoodsId);
                            if (goods != null)
                            {
                                goodsunit.Name = goods.Name;
                                goodsunit.Price = goods.Price;
                            }
                            else
                            {
                                goodsunit.Name = "No goods with this ID";
                                goodsunit.Price = 0;
                            }
                            unitlist.Add(goodsunit);
                        }
                        //return StatusCode(200, response);
                        return View(unitlist);
                    }
                    ViewBag.Message = "Order is empty";
                    return PartialView("_GetOrderUnitsForOrder");
                    //return StatusCode(204, "No ordder units");
                }
            }

                logger.LogCritical("Orders service unavailable");
            ViewBag.Message = "Orders service unavailable";
            return PartialView("_GetOrderUnitsForOrder");
            //return StatusCode(503, "Orders service unavailable");
        }

        //модификация нескольких сервисов.
        //откат
        [HttpPost("{userid}/addorder")] //добавить заказ
        public async Task<IActionResult> AddOrder(int userid)
        {
            if (userid == 0)
                return StatusCode(400, "Invalid User");
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


        [HttpPost("{userid}/addorderunit")] //добавить товар в заказ
        public async Task<IActionResult> AddOrderUnit(int? userid, int? orderid, int? goodsid)
        {
            ViewBag.UserId = userid;
            if (userid == null || orderid == null | goodsid == null)
            {
                //return StatusCode(400, "Parametrs invalid");
                string error_message = "";
                if (userid == null)
                    error_message += "User Invalid. ";
                if (orderid == null)
                    error_message += "Order invalid. ";
                if (goodsid == null)
                    error_message += "Goods invalid. ";

                ViewBag.Message = error_message;
                return View();
            }
            var response = await orderService.AddOrderUnit(userid.GetValueOrDefault(), orderid.GetValueOrDefault(), goodsid.GetValueOrDefault());
            if (response != null)
            {
                logger.LogInformation($"Attempt to add order unit, response {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var price = await warehouseService.GetGoodsPriceForId(goodsid.GetValueOrDefault());
                    if (price != null)
                    {
                        if (price != "")
                        {
                            // logger.LogInformation($"Got price from warehouse, response {response.StatusCode}");
                            int price_sum = Convert.ToInt32(price.ToString());
                            var response_2 = await orderService.AddPrice(userid.GetValueOrDefault(), orderid.GetValueOrDefault(), price_sum);
                            if (response_2 != null)
                            {
                                logger.LogInformation($"Added sum to order, response {response_2.StatusCode}");
                                ViewBag.Message = $"Order unit added to your order {orderid}";
                                return View();
                                //return Ok();
                            }
                            else
                            {
                                logger.LogCritical("Order service unavailable");
                                ViewBag.Message = "Order service unavailable";
                                return View();
                                //return StatusCode(503, "Order service unavailable");
                                //return NotFound("Service unavailable");
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Invalid goods";
                            return View();
                            //return StatusCode(400, "Invalid goods");
                        }
                    }
                    else
                    {
                        logger.LogCritical("Warehouse service unavailable");
                        ViewBag.Message = "Warehouse service unavailable";
                        return View();
                        //return StatusCode(503, "Goods service unavailable");
                        //return NotFound("Service unavailable");
                    }
                }
                else
                {
                    ViewBag.Message = response.Content?.ReadAsStringAsync()?.Result;
                    return View();
                    //return BadRequest(response.Content?.ReadAsStringAsync()?.Result);
                }
            }
            else
            {
                logger.LogCritical("Order service unavailable");
                ViewBag.Message = "Order service unavailable";
                return View();
                //return StatusCode(503, "Order service unavailable");
                //return NotFound("Service unavailable");
            }

        }

        //Читает 1 сервис, модифицирует другое
        [HttpPost("{userid}/addpayment")] //добавить платеж в заказ
        public async Task<IActionResult> AddPayment(int userid, int orderid, int sum)
        {
            if (userid == 0 || orderid == 0 || sum == 0)
                return StatusCode(400, "Bad Request");
            var orderExists = await orderService.CheckIfOrderExists(new UserOrderModel { useridId = userid, orderId = orderid });
            logger.LogInformation($"Order response: {orderExists?.StatusCode}");
            if (orderExists == null)
                return StatusCode(503, "Order service unavailable");
   
            if (!orderExists.IsSuccessStatusCode)
                    return StatusCode(400, "Order doesn't exists");
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
                    return StatusCode(400, $"Bill for order {orderid} not");
                    //return BadRequest();
                }
            }
            else
            {
                string billid ="";
                MyQeue.Retry(async () =>
                {
                using (var client = new HttpClient())
                {
                        billid = await billService.GetBillForOrder(orderid);
                        if (billid != null)
                            return true;
                            
                        return false;
                    }
                });
                if (billid != "")
                {
                    var response_4 = await billService.AddPaymentToBill(orderid, sum);
                    if (response_4 != null)
                    {
                        if (response_4.IsSuccessStatusCode)
                            return Ok();
                        return StatusCode(400, "BadRequest");
                    }
                    HttpResponseMessage response_5 = null;
                    MyQeue.Retry(async () =>
                    {
                        using (var client = new HttpClient())
                        {
                            response_5 = await billService.AddPaymentToBill(orderid, sum);
                            if (response_5 != null)
                                return true;

                            return false;
                        }
                    });
                    if (response_5.IsSuccessStatusCode)
                        return Ok();
                    return StatusCode(400, "Bad Request for add bill");

                }
                return StatusCode(400, "Bad Request for bill");
                //logger.LogCritical("Bill service unavailable");
                //return NotFound("Service unavailable");
            }
            return StatusCode(503, "Services status: " +
                    $"Order: {(orderExists != null ? "online" : "offline")};" +
                    $"Bill: {(response != null ? "online" : "offline")};");
        }


        //с очередью
        [HttpDelete("{userid}/{orderid}/delete")] //удалить заказ
        public async Task<IActionResult> RemoveOrder(int userid, int orderid)
        {
            if (userid == 0 )
                return StatusCode(400, "Invalid User");
            if (orderid == 0)
                return StatusCode(400, "Invalid Order");
            
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
                return StatusCode(400, $"There is not order with id {orderid}");
            }
            logger.LogCritical("Order service unavailable");
            return StatusCode(503, "Order service unavailable");
        }

        [HttpDelete("{orderid}/deletebill")] //удалить счет
        public async Task<IActionResult> RemoveBill( int orderid)
        {
            if (orderid == 0)
                return StatusCode(400, "Bad Request");
            var response = await billService.RemoveBillByOrder(orderid);
            if (response != null && response.IsSuccessStatusCode)
                return Ok();
            return BadRequest();
        }

        //откат действий
        [HttpDelete("{userid}/{orderid}/deleteunit")] //удалить товар из заказа
        public async Task<IActionResult> RemoveOrderUnit(int userid, int orderid, int goodsid)
        {
            if (userid == 0 || orderid == 0 || goodsid == 0)
                return StatusCode(400, "Bad Request");
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
                            return StatusCode(400, $"No goods {goodsid}");
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
