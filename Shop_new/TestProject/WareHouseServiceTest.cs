using Microsoft.VisualStudio.TestTools.UnitTesting;
using BillingService.Controllers;
using BillingService.Models;
using System.Linq;
using WareHouseService.Models;
using WareHouseService.Controllers;
using Shop_new.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Moq;
using System;
using Newtonsoft.Json;

namespace TestProject
{
    [TestClass]
    public class WareHouseServiceTest
    {
        private ILogger<GoodsController> logger;
        private GoodsDbContext dbContext;


        [TestInitialize]
        public void TestInitialize()
        {
            logger = Mock.Of<ILogger<GoodsController>>();
            dbContext = GetDbContext();
        }


        [TestMethod]
        public void GetInfForGoodNameValid()
        {
            dbContext = GetDbContext(new List<Good> { new Good { Id = 1, Name = "samsung", Count = 10, Price = 20000 } });
            var controller = GetGoodController();
            var result = controller.GetInfForGoodName("samsung",0,0);
            List<string> list = new List<string>();
            var g2 = new WareHouseModel {Id=1, Name = "samsung", Count = 10, Price = 20000 };
            list.Add(JsonConvert.SerializeObject(g2));
            var str = JsonConvert.SerializeObject(g2);
            var resstr = result.Result[0];
            Assert.IsTrue(resstr == str);
            //Assert.IsTrue(result.Result == "AmountPaid: 0");
        }

        [TestMethod]
        public void GetInfForGoodNameNOTValid()
        {
            dbContext = GetDbContext(new List<Good> { new Good { Id = 1, Name = "samsung", Count = 10, Price = 20000 } });
            var controller = GetGoodController();
            var result = controller.GetInfForGoodName("aag", 0, 0);
            List<string> list = new List<string>();
            Assert.IsTrue(result.Result.Count == list.Count);
        }



        [TestMethod]
        public void GetInfoValid()
        {
            List<Good> ll = new List<Good>();
            ll.Add( new Good { Id = 1, Name = "samsung", Count = 10, Price = 20000 } );
            ll.Add(new Good { Id = 2, Name = "Apple", Count = 2, Price = 25000 });
            dbContext = GetDbContext(ll);

            List<Good> ll2 = new List<Good>();
            ll2.Add(new Good {Name = "samsung", Count = 10, Price = 20000 });
            ll2.Add(new Good { Name = "Apple", Count = 2, Price = 25000 });
            var resStr = JsonConvert.SerializeObject(ll2);

            var controller = GetGoodController();
            var result = controller.GetInfo(0, 0);
            List<string> list = new List<string>();
            list.Add($"Name: samsung{Environment.NewLine}Price: 20000{Environment.NewLine}Count: 10");
            Assert.IsTrue(result.Result == resStr);
            //Assert.IsTrue(result.Result == "AmountPaid: 0");
        }


        [TestMethod]
        public void GetGoodsForIdValid()
        {
            List<Good> ll = new List<Good>();
            var g = new Good { Id = 1, Name = "samsung", Count = 10, Price = 20000 };
            var g2 = new WareHouseModel { Id = 1, Name = "samsung", Count = 10, Price = 20000 };
            ll.Add(g);
            dbContext = GetDbContext(ll);
            var controller = GetGoodController();
            var result = controller.GetGoodsForId(1);
            var str = JsonConvert.SerializeObject(g2);
            Assert.IsTrue(result.Result == str);
        }


        [TestMethod]
        public void GetGoodsForIdNOTValid()
        {
            List<Good> ll = new List<Good>();
            var g = new Good { Id = 1, Name = "samsung", Count = 10, Price = 20000 };
            ll.Add(g);
            dbContext = GetDbContext(ll);
            var controller = GetGoodController();
            var result = controller.GetGoodsForId(2);
            Assert.IsTrue(result.Result == "");
        }


        [TestMethod]
        public void GetGoodsForNameValid()
        {
            List<Good> ll = new List<Good>();
            var g = new Good { Id = 1, Name = "samsung", Count = 10, Price = 20000 };
            var g2 = new WareHouseModel {Id = 1, Name = "samsung", Count = 10, Price = 20000 };
            ll.Add(g);
            dbContext = GetDbContext(ll);
            var controller = GetGoodController();
            var result = controller.GetGoodsForName("samsung");
            var str = JsonConvert.SerializeObject(g2);
            Assert.IsTrue(result.Result == str);
        }


        [TestMethod]
        public void GetGoodsForNameNOTValid()
        {
            List<Good> ll = new List<Good>();
            var g = new Good { Id = 1, Name = "samsung", Count = 10, Price = 20000 };
            ll.Add(g);
            dbContext = GetDbContext(ll);
            var controller = GetGoodController();
            var result = controller.GetGoodsForName("samsung1");
           // var str = JsonConvert.SerializeObject(g);
            Assert.IsTrue(result.Result == "");
        }

        [TestMethod]
        public void GetGoodsPriceForIdValid()
        {
            List<Good> ll = new List<Good>();
            var g = new Good { Id = 1, Name = "samsung", Count = 10, Price = 20000 };
            ll.Add(g);
            dbContext = GetDbContext(ll);
            var controller = GetGoodController();
            var result = controller.GetGoodsPriceForId(1);
            var str = "20000";
            Assert.IsTrue(result.Result == str);
        }


        [TestMethod]
        public void GetGoodsPriceForIdNOTValid()
        {
            List<Good> ll = new List<Good>();
            var g = new Good { Id = 1, Name = "samsung", Count = 10, Price = 20000 };
            ll.Add(g);
            dbContext = GetDbContext(ll);
            var controller = GetGoodController();
            var result = controller.GetGoodsPriceForId(2);
            var str = "";
            Assert.IsTrue(result.Result == str);
        }

        [TestMethod]
        public void GetAddGoodsValid()
        {
            
        }

        //[TestMethod]
        //public void TestAddBillValid()
        //{
        //    var bills = new List<Billing>();
        //    dbContext = GetDbContext(bills);
        //    var controller = GetBillingController();

        //    var result = controller.AddBill(1).Result;
        //    Assert.IsTrue(result is OkResult);
        //    Assert.IsTrue(bills.Any(q => q.UserId == 1));
        //}


      




        #region Support
        private GoodsController GetGoodController()
        {
            return new GoodsController(dbContext, logger);
        }

        private GoodsDbContext GetDbContext(List<Good> goods = null)
        {
            if (goods == null)
                goods = new List<Good>();
            return Mock.Of<GoodsDbContext>(db =>
                db.Goods == GetGoods(goods));
        }


        private DbSet<Good> GetGoods(List<Good> goods)
        {
            var goodsQueryable = goods.AsQueryable();
            var mockSet = new Mock<DbSet<Good>>();
            var mockSetQueryable = mockSet.As<IQueryable<Good>>();
            mockSetQueryable.Setup(m => m.Provider).Returns(goodsQueryable.Provider);
            mockSetQueryable.Setup(m => m.Expression).Returns(goodsQueryable.Expression);
            mockSetQueryable.Setup(m => m.ElementType).Returns(goodsQueryable.ElementType);
            mockSetQueryable.Setup(m => m.GetEnumerator()).Returns(goods.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Good>())).Callback<Good>(u => goods.Add(u));
            mockSet.Setup(d => d.Remove(It.IsAny<Good>())).Callback<Good>(u => goods.Remove(u));
            return mockSet.Object;
        }
    }
    #endregion
}
