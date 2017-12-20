using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop_new.Services;
using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_new.Controllers
{
    [Route("admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private StatisticService statisticsService;

        public AdminController(StatisticService statisticsService)
        {
            this.statisticsService = statisticsService;
        }

        [HttpGet("requests")]
        public async Task<IActionResult> RequestsStatistic()
        {
            return View();
        }

        [HttpGet("requests/values")]
        public async Task<List<RequestModel>> Requests()
        {
            var b = await statisticsService.GetRequests();
            return b;
        }

        [HttpGet("requests/values/detail")]
        public async Task<List<RequestDetailModel>> RequestsDetail()
        {
            var b = await statisticsService.GetRequestsDetailed();
            return b;
        }

        [HttpGet("logins")]
        public async Task<IActionResult> LoginStatistic()
        {
            return View();
        }

        [HttpGet("logins/details")]
        public async Task<List<LoginModel>> LoginDetail()
        {
            var a = await statisticsService.GetLoginDetailed();
            return a;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> OrderStatistic()
        {
            return View();
        }

        [HttpGet("orders/values")]
        public async Task<List<OrderModel>> Orders()
        {
            var b = await statisticsService.GetOrdersAdditions();
            return b;
        }

        [HttpGet("orders/values/detail")]
        public async Task<List<OrderDetailModel>> OrderAddDetail()
        {
            var b = await statisticsService.GetOrdersAdditionsDetail();
            return b;
        }
    }
}
