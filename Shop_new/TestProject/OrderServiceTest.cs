using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderService.Controllers;
using OrderService.Models;
using System.Linq;
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
    class OrderServiceTest
    {
        private ILogger<OrderController> logger;
        private OrderDbContext dbContext;


        [TestInitialize]
        public void TestInitialize()
        {
            logger = Mock.Of<ILogger<OrderController>>();
            dbContext = GetDbContext();
        }

        [TestMethod]
        public void TestGetOrdersForUserValid()
        {
            DateTime date = DateTime.Now;
            dbContext = GetDbContext(new List<Order> { new Order {Id=1, Date=date, UserId=1, TotalSum = 0 } });
            var controller = GetOrderController();
            //string str = "Order: 1" + ", Date: " + date.ToString();
            var ordd = new Order { Id = 1, Date = date, TotalSum = 0, UserId = 1 };
            string str = JsonConvert.SerializeObject(ordd);
            var result = controller.GetOrdersForUser(1, 0, 0).Result;
            Assert.IsTrue(result == str);
        }



        [TestMethod]
        public void TestAddOrderValid()
        {
            var Orders = new List<Order>();
            dbContext = GetDbContext(Orders);
            var controller = GetOrderController();

            var result = controller.AddOrder(1).Result;
            //Assert.IsTrue(result is OkResult);
            //Assert.IsTrue(Orders.Any(q => q.UserId == 1));
            Assert.IsTrue(result == 1);
        }


        [TestMethod]
        public void TestAddOrderUnitValid()
        {
            var Orders = new List<Order>();
            var ord = new Order { Id = 1, Date = DateTime.Now, TotalSum = 0, UserId = 1 };
            Orders.Add(ord);
            var OrderUnits = new List<OrderUnit>();
            
            var unit = new OrderUnit { Id = 1, Count = 1, GoodsId = 2, OrderId = 1 };
            OrderUnits.Add(unit);
            dbContext = GetDbContext(Orders, OrderUnits);

            var controller = GetOrderController();

            var result = controller.AddOrder(1).Result;
            var result_2 = controller.AddOrderUnit(1, 1, 2);
            //Assert.IsTrue(result is OkResult);
            //Assert.IsTrue(Orders.Any(q => q.UserId == 1));
            Assert.IsTrue(result == 1);
            //Assert.IsTrue(result_2 is OkResult);
        }



        #region Support
        private OrderController GetOrderController()
        {
            return new OrderController(dbContext, logger);
        }

        private OrderDbContext GetDbContext(List<Order> Orders = null, List<OrderUnit> OrderUnits = null)
        {
            if (Orders == null)
                 Orders = new List<Order>();
            return Mock.Of<OrderDbContext>(db =>
                db.Orders == GetOrders(Orders) &&
                db.OrderUnits == GetOrderUnits(OrderUnits));
        }

        private DbSet<OrderUnit> GetOrderUnits(List<OrderUnit> orderUnits)
        {
            var OrderUnitsQueryable = orderUnits.AsQueryable();
            var mockSet = new Mock<DbSet<OrderUnit>>();
            var mockSetQueryable = mockSet.As<IQueryable<OrderUnit>>();
            mockSetQueryable.Setup(m => m.Provider).Returns(OrderUnitsQueryable.Provider);
            mockSetQueryable.Setup(m => m.Expression).Returns(OrderUnitsQueryable.Expression);
            mockSetQueryable.Setup(m => m.ElementType).Returns(OrderUnitsQueryable.ElementType);
            mockSetQueryable.Setup(m => m.GetEnumerator()).Returns(orderUnits.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<OrderUnit>())).Callback<OrderUnit>(u => orderUnits.Add(u));
            mockSet.Setup(d => d.Remove(It.IsAny<OrderUnit>())).Callback<OrderUnit>(u => orderUnits.Remove(u));
            return mockSet.Object;
        }

        private DbSet<Order> GetOrders(List<Order> Orders)
        {
            var OrdersQueryable = Orders.AsQueryable();
            var mockSet = new Mock<DbSet<Order>>();
            var mockSetQueryable = mockSet.As<IQueryable<Order>>();
            mockSetQueryable.Setup(m => m.Provider).Returns(OrdersQueryable.Provider);
            mockSetQueryable.Setup(m => m.Expression).Returns(OrdersQueryable.Expression);
            mockSetQueryable.Setup(m => m.ElementType).Returns(OrdersQueryable.ElementType);
            mockSetQueryable.Setup(m => m.GetEnumerator()).Returns(Orders.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Order>())).Callback<Order>(u => Orders.Add(u));
            mockSet.Setup(d => d.Remove(It.IsAny<Order>())).Callback<Order>(u => Orders.Remove(u));
            return mockSet.Object;
        }
    }
    #endregion
}

