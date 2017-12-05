using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SubscriptionManager;
using SubscriptionManager.Controllers;
using SubscriptionManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    [TestClass]
    public class SubscriptionsManagerTests
    {
        private const string author = "author";
        private const string subscriber = "subscriber";
        private ILogger<SubscriptionsController> logger;
        private ApplicationDbContext dbContext;

        [TestInitialize]
        public void TestInitialize()
        {
            logger = Mock.Of<ILogger<SubscriptionsController>>();
            dbContext = GetDbContext();
        }

        [TestMethod]
        public void TestGetAuthorsForUserValid()
        {
            dbContext = GetDbContext(new List<Subscription> { new Subscription { Author = author, Subscriber = subscriber } });
            var subscriptionsController = GetSubscriptionsController();

            var result = subscriptionsController.GetAuthorsForName(subscriber, 0, 0).Result;
            //Assert.IsTrue(result.First() == author);
        }

        [TestMethod]
        public void TestGetAuthorsForUserNotValid()
        {
            var subscriptionsController = GetSubscriptionsController();

            var result = subscriptionsController.GetAuthorsForName(subscriber, 0, 0).Result;
            //Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void TestAddSubscriptionValid()
        {
            var subscriptions = new List<Subscription>();
            dbContext = GetDbContext(subscriptions);
            var subscriptionsController = GetSubscriptionsController();

            var result = subscriptionsController.AddSubscription(subscriber, author).Result;
            Assert.IsTrue(result is OkResult);
            Assert.IsTrue(subscriptions.Any(s => s.Author == author && s.Subscriber == subscriber));
        }

        [TestMethod]
        public void TestAddSubscriptionNotValid()
        {
            dbContext = GetDbContext(new List<Subscription> { new Subscription { Author = author, Subscriber = subscriber } });
            var subscriptionsController = GetSubscriptionsController();

            var result = subscriptionsController.AddSubscription(subscriber, author).Result;
            Assert.IsFalse(result is OkResult);
        }

        [TestMethod]
        public void TestRemoveSubscriptionValid()
        {
            var subscriptions = new List<Subscription> { new Subscription { Author = author, Subscriber = subscriber } };
            dbContext = GetDbContext(subscriptions);
            var subscriptionsController = GetSubscriptionsController();

            var result = subscriptionsController.RemoveSubscription(subscriber, author).Result;
            Assert.IsTrue(result is OkResult);
            Assert.IsTrue(subscriptions.Count == 0);
        }

        [TestMethod]
        public void TestRemoveSubscriptionNotValid()
        {
            var subscriptionsController = GetSubscriptionsController();

            var result = subscriptionsController.RemoveSubscription(subscriber, author).Result;
            Assert.IsFalse(result is OkResult);
        }

        #region Support
        private SubscriptionsController GetSubscriptionsController()
        {
            return new SubscriptionsController(dbContext, logger);
        }

        private ApplicationDbContext GetDbContext(List<Subscription> subscriptions = null)
        {
            if (subscriptions == null)
                subscriptions = new List<Subscription>();
            return Mock.Of<ApplicationDbContext>(db =>
                db.Subscriptions == GetSubscriptions(subscriptions));
        }

        private DbSet<Subscription> GetSubscriptions(List<Subscription> subscriptions)
        {
            var subscriptionsQueryable = subscriptions.AsQueryable();
            var mockSet = new Mock<DbSet<Subscription>>();
            var mockSetQueryable = mockSet.As<IQueryable<Subscription>>();
            mockSetQueryable.Setup(m => m.Provider).Returns(subscriptionsQueryable.Provider);
            mockSetQueryable.Setup(m => m.Expression).Returns(subscriptionsQueryable.Expression);
            mockSetQueryable.Setup(m => m.ElementType).Returns(subscriptionsQueryable.ElementType);
            mockSetQueryable.Setup(m => m.GetEnumerator()).Returns(subscriptions.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Subscription>())).Callback<Subscription>(u => subscriptions.Add(u));
            mockSet.Setup(d => d.Remove(It.IsAny<Subscription>())).Callback<Subscription>(u => subscriptions.Remove(u));
            return mockSet.Object;
        }
        #endregion
    }
}
