using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BillingService.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BillingService.Controllers
{
    [Route("api")]
    public class BillController : Controller
    {
        private BillDbContext db;
        private ILogger<BillController> logger;

        public BillController(BillDbContext db, ILogger<BillController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        [HttpGet("{orderid}")]
        public async Task<string> GetBillForOrder(int orderid)
        {
            logger.LogDebug($"Retriving bill for order {orderid}");
            var result = db.Billings.FirstOrDefault(q => q.UserId == orderid);
            if (result != null)
            {
                logger.LogDebug($"Found bill for order {orderid} with Amount paid = {result.AmmountPaid}");
                string str = $"AmountPaid: {result.AmmountPaid}";
                return str;
            }
            else
            {
                logger.LogDebug($" bill for order {orderid} not found");
                return "";
            }
        }

        [HttpGet("bills")]
        public async Task<string> GetBills(int page, int perpage)
        {
            // var good = db.Goods.Select(q => q);
            var bills = db.Billings.Select(q => q);
            if (perpage != 0 && page != 0)
            {
                logger.LogDebug($"Skipping {perpage * page} goods due to pagination");
                bills = bills.Skip(perpage * page);
            }
            if (perpage != 0)
            {
                logger.LogDebug($"Retrieving at max {perpage} goods");
                bills = bills.Take(perpage);
            }
            // var list = new List<WareHouseModel>();
            List<Billing> resList = new List<Billing>();
            foreach (var el in bills)
            {
                resList.Add(new Billing { UserId=el.UserId, AmmountPaid=el.AmmountPaid });
            }
            return JsonConvert.SerializeObject(resList);
        }


        [HttpPost("{orderid}")]
        public async Task<IActionResult> AddBill(int orderid)
        {
            logger.LogDebug($"Adding bill for order {orderid}");
            var bill1 = db.Billings.FirstOrDefault(q => q.UserId == orderid);
            if (bill1 == null)
            {
                var bill = new Billing { UserId = orderid, AmmountPaid = 0 };
                var state = db.Billings.Add(bill)?.State;
                logger.LogDebug($"Bill for order {orderid} added with state {state}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"Bill for order {orderid} already exists");
            return BadRequest();
        }

        //внести сумму для заказа
        [HttpPost("{orderid}/pay")]
        public async Task<IActionResult> AddPaid([FromRoute]int orderid, int sum)
        {
            logger.LogDebug($"Adding aid for order {orderid}");
            var bill = db.Billings.FirstOrDefault(q => q.UserId == orderid);
            if (bill != null)
            {
                bill.AmmountPaid += sum;
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"Bill for order {orderid} not found");
            return BadRequest();
        }


        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> RemoveBill(int id)
        {
            logger.LogDebug($"Removing bill {id}");
            var bill = db.Billings.FirstOrDefault(q => q.Id == id);
            if (bill != null)
            {
                logger.LogDebug($"Bill {id} exists in database. Deleting...");
                var state = db.Remove(bill)?.State;
                logger.LogDebug($"Bill {id} removed with state {state}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"Bill {id} not found");
            return BadRequest();
        }

        [HttpDelete("{orderid}/byorder")]
        public async Task<IActionResult> RemoveBillWithOrderId(int orderid)
        {
            logger.LogDebug($"Removing bill {orderid}");
            var bill = db.Billings.FirstOrDefault(q => q.UserId == orderid);
            if (bill != null)
            {
                logger.LogDebug($"Bill for order {orderid} exists in database. Deleting...");
                var state = db.Remove(bill)?.State;
                logger.LogDebug($"Bill for order {orderid} removed with state {state}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"Bill for order {orderid} not found");
            return BadRequest();
        }

    }


}
