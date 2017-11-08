using Microsoft.VisualStudio.TestTools.UnitTesting;
using BillingService.Controllers;
using BillingService.Models;
using System.Linq;
using OrderService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Moq;

namespace TestProject
{
    [TestClass]
    public class BillingServiceTest
    {
        private ILogger<BillController> logger;
        private BillDbContext dbContext;


        [TestInitialize]
        public void TestInitialize()
        {
            logger = Mock.Of<ILogger<BillController>>();
            dbContext = GetDbContext();
        }


        [TestMethod]
        public void GetBillForOrderValid()
        {
            dbContext = GetDbContext(new List<Billing> { new Billing { Id = 1, UserId = 1, AmmountPaid = 0 } });
            var controller = GetBillingController();
            var result = controller.GetBillForOrder(1);
            Assert.IsTrue(result.Result == "AmountPaid: 0");
        }

        [TestMethod]
        public void GetBillForOrderNOTValid()
        {
            var controller = GetBillingController();
            var result = controller.GetBillForOrder(1).Result;
            Assert.IsTrue(result == "");
        }

        [TestMethod]
        public void TestAddBillValid()
        {
            var bills = new List<Billing>();
            dbContext = GetDbContext(bills);
            var controller = GetBillingController();

            var result = controller.AddBill(1).Result;
            Assert.IsTrue(result is OkResult);
            Assert.IsTrue(bills.Any(q => q.UserId == 1));
        }


        [TestMethod]
        public void TestAddBillNOValid()
        {
            var bills = new List<Billing>();
            dbContext = GetDbContext(new List<Billing> { new Billing { Id = 1, UserId = 1, AmmountPaid = 10 } });
            var controller = GetBillingController();

            var result = controller.AddBill(1).Result;
            Assert.IsFalse(result is OkResult);
        }


        [TestMethod]
        public void TestRemoveBillValid()
        {
            var bills = new List<Billing> { new Billing { Id = 1, UserId = 1, AmmountPaid = 10 } };
            dbContext = GetDbContext(bills);
            var controller = GetBillingController();

            var result = controller.RemoveBill(1).Result;
            Assert.IsTrue(result is OkResult);
            //Assert.IsTrue(bills.Count == 0);
        }


        [TestMethod]
        public void TestRemoveBillNOTValid()
        {
            var controller = GetBillingController();
            var result = controller.RemoveBill(1).Result;
            Assert.IsFalse(result is OkResult);
        }



        #region Support
        private BillController GetBillingController()
        {
            return new BillController(dbContext, logger);
        }

        private BillDbContext GetDbContext(List<Billing> billings = null)
        {
            if (billings == null)
                billings = new List<Billing>();
            return Mock.Of<BillDbContext>(db =>
                db.Billings == GetBillings(billings));
        }


        private DbSet<Billing> GetBillings(List<Billing> billings)
        {
            var billingsQueryable = billings.AsQueryable();
            var mockSet = new Mock<DbSet<Billing>>();
            var mockSetQueryable = mockSet.As<IQueryable<Billing>>();
            mockSetQueryable.Setup(m => m.Provider).Returns(billingsQueryable.Provider);
            mockSetQueryable.Setup(m => m.Expression).Returns(billingsQueryable.Expression);
            mockSetQueryable.Setup(m => m.ElementType).Returns(billingsQueryable.ElementType);
            mockSetQueryable.Setup(m => m.GetEnumerator()).Returns(billings.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Billing>())).Callback<Billing>(u => billings.Add(u));
            mockSet.Setup(d => d.Remove(It.IsAny<Billing>())).Callback<Billing>(u => billings.Remove(u));
            return mockSet.Object;
        }
    }
    #endregion
}
