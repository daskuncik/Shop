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
using System.Text.RegularExpressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Shop_new.CustomAuthorisation;

namespace Shop_new.Controllers
{
    
    public class MainController : Controller
    {
        private BillService billService;
        private OrderService orderService;
        private WareHouseService warehouseService;
        private UserService userService;
        private ILogger<MainController> logger;
        private TokenStore tokenStore;


        public MainController(ILogger<MainController> logger,
            BillService _billService,
            OrderService _orderService,
            WareHouseService _warehouseService,
            UserService _userService)
        {
            this.logger = logger;
            this.billService = _billService;
            this.orderService = _orderService;
            this.warehouseService = _warehouseService;
            this.userService = _userService;
        }

        //[Authorize]
        [HttpGet("")]
        public async Task<IActionResult> Get(string username)
        {
            if (Request.Headers.Keys.Contains(CustomAuthorizationMiddleware.UserWord))
                username = string.Join(string.Empty, Request.Headers[CustomAuthorizationMiddleware.UserWord]);
            if (!string.IsNullOrWhiteSpace(username))
            {
                int userid = await userService.GetUserIdByName(username);
                ViewBag.UserId = userid;
                Dictionary<string, string> parametrs = new Dictionary<string, string>(); 
                parametrs.Add("userid", userid.ToString());
                return RedirectToAction("GetOrders", parametrs);
            }
            else
                return RedirectToAction(nameof(Register));
        }

        [HttpGet("user")]
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost("user")]
        public async Task<IActionResult> Register(string username, string password)
        {
            if (username == null || !Regex.IsMatch(username, @"\S+"))
                return BadRequest("Name not valid");

            var response = await userService.Register(username, password);
            logger.LogInformation($"Response from accounts service: {response?.StatusCode}");

            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return StatusCode(200, "");
            }
            else if (response != null)
            {
                string respContent = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                logger.LogError($"User {username} not registered, error content: {respContent}");
                return StatusCode(500, respContent);
            }
            else
            {
                logger.LogCritical("Accounts service unavailable");
                return StatusCode(503, "Accounts service unavailable");
            }
        }

        [HttpGet("login")]
        //[HttpGet("")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await userService.Login(username, password);
            if (result == null)
            {
                //return StatusCode(503, "Accounts service unavailable");
                ViewBag.Message = "Users service unavailable";
                return View();
            }
            else
            if (result.IsSuccessStatusCode)
            {

                var token = tokenStore.GetToken(username, TimeSpan.FromMinutes(10));
                Response.Cookies.Append(CustomAuthorizationMiddleware.AuthorizationWord, $"Bearer {token}");
                int userid = await userService.GetUserId(username, password);
                if (userid > 0)
                {
                    ViewBag.UserId = userid;
                    return View("GetGoods");
                }
                else
                    return View(nameof(Register));
                //var token = tokenStore.GetToken(authenticationModel.Username, TimeSpan.FromMinutes(10));
            }
            else
                return StatusCode(500, result.Content.ReadAsStringAsync().Result);


            //var result = await userService.CheckIfUserExists(username);
            //if (result.StatusCode != System.Net.HttpStatusCode.OK)
            //{
            //    ViewBag.Message = $"No User with name {username}";
            //    return View("Register");
            //}
            //await Authenticate(username);
            //ViewBag.Message = "Success";
            //return View("_Register");
            //return View("Error", new ErrorModel(result));
            //return RedirectToAction(nameof(Index), new IndexModel { Username = username });
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.Keys.Contains(CustomAuthorizationMiddleware.AuthorizationWord))
            {
                var cookie = Request.Cookies[CustomAuthorizationMiddleware.AuthorizationWord];
                Response.Cookies.Append(CustomAuthorizationMiddleware.AuthorizationWord, cookie, new Microsoft.AspNetCore.Http.CookieOptions { Expires = DateTime.Now.AddDays(-1) });
            }
            return RedirectToAction(nameof(Register));
        }

        //[Authorize]
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
                ViewBag.Message = "Invalid f Pages";
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
                            //AmountPaid = Convert.ToInt32(response_bill)

                        };
                        if (response_bill == null)
                            model.AmountPaid = -1;
                        else
                            model.AmountPaid = Convert.ToInt32(response_bill);
                        resultList.Add(model);
                    }
                }
                ViewBag.UserId = userid;
                ViewBag.Orders = resultList;
                logger.LogInformation($"Number of orders for user {userid.GetValueOrDefault()}: {response.Count}");
                // return StatusCode(200, response);
                //return response;
                //return StatusCode(200, response); //JsonConvert.SerializeObject(response);
                return View("GetOrders", resultList);
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

        //[Authorize]
        [HttpGet("{userid}/goods")] //получить все товары
        public async Task<IActionResult> GetGoods(int? userid, int page = 0, int perpage = 0)
        {
            ViewBag.UserId = userid;
            List<OrderModel> response_order = await orderService.GetOrdersForUser(userid.GetValueOrDefault(), 0, 0);
            if (response_order != null && response_order.Count() > 0)
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

        [Authorize]
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

        //[Authorize]
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
            if (response1 == null)
            {
                logger.LogCritical("Orders service unavailable");
                ViewBag.Message = "Orders service unavailable";
                return PartialView("_GetOrderUnitsForOrder");
            }
            if (response1.Id == 0 && response1.UserId == 0 && response1.TotalSum == 0)
            {
                ViewBag.Message = $"Order {orderid} not found";
                return PartialView("_GetOrderUnitsForOrder");
            }
            ViewBag.Date = response1.Date;
            ViewBag.OrderId = response1.Id;
            ViewBag.TotalSum = response1.TotalSum;
            List<OrderUnitModel> response = await orderService.GetUnitsForOrder(userid.GetValueOrDefault(), orderid.GetValueOrDefault());
            if (response == null)
            {
                ViewBag.Message = "Orders service unavailable";
                return PartialView("_GetOrderUnitsForOrder");
            }
                        //logger.LogInformation($"Number of orders for user {userid}: {response.Count}");
                        //return response;
               // if (response.Count() > 0)
               // {
            if (response.Count() == 1 && response[0].Count == 0 && response[0].GoodsId == 0 && response[0].OrderId == 0)
            {
                ViewBag.Message = "Order is empty";
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

            var bill = await billService.GetBillForOrder(orderid.GetValueOrDefault());
            if (bill == null)
                ViewBag.Bill = "Bill Service unavailable";
            else
            {
                if (bill != "")
                    ViewBag.Bill = "You paid: " + bill;
                else
                    ViewBag.Bill = "Invalid Bill";
            }
            //return StatusCode(200, response);
            return View(unitlist);
                //}
                    //}
               //}
                
             //return StatusCode(204, "No ordder units");   
            //return StatusCode(503, "Orders service unavailable");
        }


        //модификация нескольких сервисов.
        //откат
        //[Authorize]
        [HttpPost("{userid}/addorder")] //добавить заказ
        public async Task<IActionResult> AddOrder(int? userid)
        {
            if (userid == null)
            {
                ViewBag.Message = "Invalid User";
                return View();
                //return StatusCode(400, "Invalid User");
            }

            ViewBag.UserId = userid;
            var response = await orderService.AddOrder(userid.GetValueOrDefault());
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
                            ViewBag.Message = "Order & Bill successfully created";
                            return View();
                            //return Ok();
                        }
                        //else
                        var response_3 = await orderService.RemoveOrder(userid.GetValueOrDefault(), orderid);
                        ViewBag.Message = "Error during creating bill. Order wasn't created";
                        return View();
                        //return StatusCode(503, response.Content?.ReadAsStringAsync()?.Result);
                        //return BadRequest(response_2.Content?.ReadAsStringAsync()?.Result);
                    }
                    else
                    {
                        var response_3 = await orderService.RemoveOrder(userid.GetValueOrDefault(), orderid);
                        if (response_3 != null)
                        {
                            ViewBag.Message = $"Bill servise unavailable. Rollback status for removig order: {(response_3.IsSuccessStatusCode ? "ok" : "failed")}";
                            return View();
                            //return StatusCode(503, $"Bill servise unavailable. Rollback status for removig order: {(response_3.IsSuccessStatusCode ? "ok" : "failed")}");
                        }
                        ViewBag.Message = $"Bill servise unavailable. Order service unavailable: cann't remove created order";
                        return View();
                        //return StatusCode(400, 
                        // logger.LogCritical("Bills service unavailable");
                        //return NotFound("Service unavailable");
                    }
                }
                else
                {
                    ViewBag.Message = "Error during creating of order";
                    return View();
                    //return StatusCode(503, response.Content?.ReadAsStringAsync()?.Result);
                    //return BadRequest(response.Content?.ReadAsStringAsync()?.Result);
                }
            }
            else
            {
                logger.LogCritical("Orders service unavailable");
                ViewBag.Message = "Order Service unavailable";
                return View();
                //return NotFound("Service unavailable");
            }
        }

        //[Authorize]
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

       // [Authorize]
        [HttpGet("{userid}/addpayment")] //добавить платеж в заказ (форма для внесения суммы)
        public async Task<IActionResult> AddPayment(int? userid, int? orderid)
        {
            string message = "";
            if (userid == null)
                message += "Invalid User";
            if (orderid == null)
                message += "Invalid order";
            if (message == "")
                ViewBag.Message = $"Enter the amount for order {orderid.GetValueOrDefault()}";
            else
                ViewBag.Message = message;
            ViewBag.UserId = userid.GetValueOrDefault();
            ViewBag.OrderId = orderid.GetValueOrDefault();
            return View();
        }

        //Читает 1 сервис, модифицирует другое
        //[Authorize]
        [HttpPost("{userid}/addpayment")] //добавить платеж в заказ
        public async Task<IActionResult> AddPayment(int? userid, int? orderid, int? sum)
        {
            if (userid == 0 || orderid == 0 || sum == 0)
            {
                string message = "";
                if (userid == null)
                    message += "Invalid User. ";
                else
                    ViewBag.UserId = userid;

                if (orderid == null)
                    message += "Invalid order. ";

                if (sum == null)
                    message += "Invalid Sum.";
                //return StatusCode(400, "Bad Request");
            }
            ViewBag.UserId = userid;
            ViewBag.OrderId = orderid;
            //var orderExists = await orderService.CheckIfOrderExists(new UserOrderModel { useridId = userid, orderId = orderid });
            var order = await orderService.GetOrder(userid.GetValueOrDefault(), orderid.GetValueOrDefault());
            //logger.LogInformation($"Order response: {orderExists?.StatusCode}");
            if (order == null)
            {
                ViewBag.Message = "Order service unavailable";
                return PartialView("_AddPayment");
                //return StatusCode(503, "Order service unavailable");
            }

            if (order.Id==0 && order.TotalSum == 0 & order.UserId == 0)
            {
                ViewBag.Message = "Order doesn't exist";
                return PartialView("_AddPayment");
                //return StatusCode(400, "Order doesn't exists");
            }
            //return NotFound("Order doesn't exists");

            var response = await billService.GetBillForOrder(orderid.GetValueOrDefault());
            if (response != null)
            {
                if (response != "")
                {
                    var response_2 = await billService.AddPaymentToBill(orderid.GetValueOrDefault(), sum.GetValueOrDefault(), order.TotalSum);
                    if (response_2 != null)
                    {
                        if (response_2.IsSuccessStatusCode)
                        {
                            ViewBag.Message = "You added payment success";
                            return PartialView("_AddPayment");
                            //return Ok();
                        }
                        
                        if (response_2.StatusCode == System.Net.HttpStatusCode.Accepted)
                        {
                            ViewBag.Message = "Your order is successfuly closed";
                            //сделать здесь декремент тех вещей на складе, которые есть в этом заказе
                            return PartialView("_AddPayment");
                        }
                            //return BadRequest(response_2.Content?.ReadAsStringAsync()?.Result);
                        if (response_2.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            ViewBag.Message = "The amount paid is more than necessary for an order. Please correct your sum";
                            return PartialView("_AddPayment");
                        }
                        ViewBag.Message = $"Bill for order {orderid.GetValueOrDefault()} not found";
                        return PartialView("_AddPayment");

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
                    ViewBag.Message = $"Bill for order {orderid.GetValueOrDefault()} not found";
                    return PartialView("_AddPayment");
                    // logger.LogCritical($"Bill for order {orderid} not");
                    //return StatusCode(400, $"Bill for order {orderid} not");
                   // //return BadRequest();
                }
            }
            else
            {
                string billid ="";
                MyQeue.Retry(async () =>
                {
                using (var client = new HttpClient())
                {
                        billid = await billService.GetBillForOrder(orderid.GetValueOrDefault());
                        if (billid != null)
                            return true;
                            
                        return false;
                    }
                });
                if (billid != "")
                {
                    var response_4 = await billService.AddPaymentToBill(orderid.GetValueOrDefault(), sum.GetValueOrDefault(), order.TotalSum);
                    if (response_4 != null)
                    {
                        if (response_4.IsSuccessStatusCode)
                        {
                            ViewBag.Message = "You added payment success";
                            return PartialView("_AddPayment");
                            //return Ok();
                        }
                        if (response_4.StatusCode == System.Net.HttpStatusCode.Accepted)
                        {
                            ViewBag.Message = "Your order is successfuly closed";
                            //сделать здесь декремент тех вещей на складе, которые есть в этом заказе
                            return PartialView("_AddPayment");
                        }
                        if (response_4.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            ViewBag.Message = "The amount paid is more than necessary for an order. Please correct your sum";
                            return PartialView("_AddPayment");
                        }
                        ViewBag.Message = $"Bill for order {orderid.GetValueOrDefault()} not found";
                        return PartialView("_AddPayment");
                    }
                    HttpResponseMessage response_5 = null;
                    MyQeue.Retry(async () =>
                    {
                        using (var client = new HttpClient())
                        {
                            response_5 = await billService.AddPaymentToBill(orderid.GetValueOrDefault(), sum.GetValueOrDefault(), order.TotalSum);
                            if (response_5 != null)
                                return true;

                            return false;
                        }
                    });
                    if (response_5.IsSuccessStatusCode)
                    {
                        ViewBag.Message = "You added payment success";
                        return PartialView("_AddPayment");
                    }
                    //return StatusCode(400, "Bad Request for add bill");
                    if (response_5.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        ViewBag.Message = "Your order is successfuly closed";
                        //сделать здесь декремент тех вещей на складе, которые есть в этом заказе
                        return PartialView("_AddPayment");
                    }
                    if (response_5.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        ViewBag.Message = "The amount paid is more than necessary for an order. Please correct your sum";
                        return PartialView("_AddPayment");
                    }
                    ViewBag.Message = $"Bill for order {orderid.GetValueOrDefault()} not found";
                    return PartialView("_AddPayment");

                }
                //return StatusCode(400, "Bad Request for bill");
                ViewBag.Message = $"Bill for order {orderid.GetValueOrDefault()} not found";
                return PartialView("_AddPayment");
            }
            return StatusCode(503, "Services status: " +
                    $"Order: {(order != null ? "online" : "offline")};" +
                    $"Bill: {(response != null ? "online" : "offline")};");
        }


        //с очередью
        //[Authorize]
        [HttpPost("{userid}/{orderid}/delete")] //удалить заказ
        public async Task<IActionResult> RemoveOrder(int? userid, int? orderid)
        {
            string message = "";
            if (userid == null)
                message += "Invalid User";
            //return StatusCode(400, "Invalid User");
            if (orderid == null)
                message += "Invalid Order";
            //return StatusCode(400, "Invalid Order");
            if (message != "")
            {
                ViewBag.Message = message;
                return View();
            }
            ViewBag.UserId = userid;
            ViewBag.OrderId = orderid;
            
            var response = await orderService.RemoveOrder(userid.GetValueOrDefault(), orderid.GetValueOrDefault());
            if (response != null)
            {
                logger.LogInformation($"Attempt to remove order {orderid.GetValueOrDefault()}, response {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var response_2 = await billService.RemoveBillByOrder(orderid.GetValueOrDefault());
                    if (response_2 != null && response_2.IsSuccessStatusCode)
                    {
                        ViewBag.Message = $"Order {orderid.GetValueOrDefault()} & bill was successfully removed";
                        return View();
                        //return Ok();
                    }

                    MyQeue.Retry(async () =>
                    {
                        using (var client = new HttpClient())
                        {// try catch здесь
                            try
                            {
                                var response_3 = await billService.RemoveBillByOrder(orderid.GetValueOrDefault());
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
                    ViewBag.Message = "Services status: " +
                        $"Order: {(response != null ? "online" : "offline")};   " +
                        $"Bill: {(response_2 != null ? "online" : "offline")}; Status: {response_2?.StatusCode}";
                    return View();
                    //return StatusCode(503, "Services status: " +
                    // $"Order: {(response != null ? "online" : "offline")};   " +
                    // $"Bill: {(response_2 != null ? "online" : "offline")}; Status: {response_2?.StatusCode}");

                }
                else
                {
                    ViewBag.Message = $"There is not order with id {orderid.GetValueOrDefault()}";
                    return View();
                }
                //return StatusCode(400, $"There is not order with id {orderid}");
            }
            logger.LogCritical("Order service unavailable");
            //return StatusCode(503, "Order service unavailable");
            ViewBag.Message = "Order service unavailable";
            return View();
        }

        //[Authorize]
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
        //[Authorize]
        [HttpPost("{userid}/{orderid}/deleteunit")] //удалить товар из заказа
        public async Task<IActionResult> RemoveOrderUnit(int? userid, int? orderid, int? goodsid)
        {
            ViewData["Title"] = "Remove unit";
            if (userid == 0 || orderid == 0 || goodsid == 0)
            {
                string message = "";
                if (userid == null)
                {
                    ViewBag.UserId = null;
                    message += "Invalid User. ";
                }
                else
                    ViewBag.UserId = userid;
                if (orderid == null)
                    message += "Invalid order. ";
                if (goodsid == null)
                    message += "Invalid goods.";
                ViewBag.Message = message;
                return PartialView("_GetOrders");
            }

            ViewBag.UserId = userid;
            ViewBag.OrderId = orderid;
            ViewBag.GoodsId = goodsid;
            var response = await orderService.RemoveOrderUnit(orderid.GetValueOrDefault(), goodsid.GetValueOrDefault());
            if (response == null)
            {
                ViewBag.Message = "Order service unavailable";
                return View();
            }
            logger.LogInformation($"Attempt to remove order {orderid.GetValueOrDefault()}, response {response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                var price_resp = await warehouseService.GetGoodsPriceForId(goodsid.GetValueOrDefault());
                if (price_resp == null)
                {
                    var restore_order = await orderService.AddOrderUnit(userid.GetValueOrDefault(), orderid.GetValueOrDefault(), goodsid.GetValueOrDefault());
                    if (restore_order == null)
                    {
                        ViewBag.Message = $"Order service unavailable. Rollback error for order unit in order {orderid.GetValueOrDefault()}";
                        return View();
                    }
                    if (restore_order.IsSuccessStatusCode)
                    {
                        ViewBag.Message = $"Goods service unavailable. Rollback order unit in order {orderid.GetValueOrDefault()}";
                        return View();
                    }
                }
                //return StatusCode(503, $"Goods service unavailable. Rollback order unit in order {orderid}");

                if (price_resp == "")
                {
                    ViewBag.Message = $"No goods {goodsid.GetValueOrDefault()}";
                    return View();
                }
                var price = Convert.ToInt32(price_resp);
                var subSum = await orderService.SubTotalSum(userid.GetValueOrDefault(), orderid.GetValueOrDefault(), price);
                if (subSum == null)
                {
                    logger.LogCritical("Order service unavailable");
                    ViewBag.Message = "Order service unavailable";
                    return View();
                    //return NotFound("Service unavailable");
                }
                if (subSum.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Removing was successful";
                    return View();
                }
                else
                {
                    var restore_order = await orderService.AddOrderUnit(userid.GetValueOrDefault(), orderid.GetValueOrDefault(), goodsid.GetValueOrDefault());
                    if (restore_order != null && restore_order.IsSuccessStatusCode)
                    {
                        ViewBag.Message = $"Subtraction Error. Rollback order unit in order {orderid.GetValueOrDefault()}";
                        return View();
                        //return StatusCode(503, $"Subtraction Error. Rollback order unit in order {orderid}");
                    }
                    ViewBag.Message = $"Order service rollback error";
                    return View();
                    //return StatusCode(503, "Order service rollback error");
                }
                //}
            }
            ViewBag.Message = "Removing Error";
            return View();
            
        }


        private async Task Authenticate(string userName)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

    }
}
