using Gateway.Controllers;
using Gateway.Models;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class GatewayTests
    {
        private ILogger<GatewayController> logger;
        private IAccountsService accountsService;
        private ISubscriptionsService subscriptionsService;
        private INewsService newsService;

        [TestInitialize]
        public void TestInitialize()
        {
            var loggerMock = new Mock<ILogger<GatewayController>>();
            logger = loggerMock.Object;
            accountsService = GetAccountsService();
            newsService = GetNewsService();
            subscriptionsService = GetSubscriptionsService();
        }

        [TestMethod]
        public void TestRegisterValid()
        {
            accountsService = GetAccountsService(registerCode: HttpStatusCode.OK);
            var mainController = GetMainController();

            var result = mainController.Register(Mock.Of<UserModel>(lm => lm.Username == "User")).Result;
            Assert.IsTrue(result is OkResult);
        }

        [TestMethod]
        public void TestRegisterNotValid()
        {
            accountsService = GetAccountsService(registerCode: HttpStatusCode.Unauthorized);
            var mainController = GetMainController();

            var result = mainController.Register(Mock.Of<UserModel>(lm => lm.Username == "User")).Result;
            Assert.IsTrue(result is BadRequestObjectResult);
        }

        [TestMethod]
        public void TestRegosterNoService()
        {
            accountsService = GetEmptyAccountsService();
            var mainController = GetMainController();

            var result = mainController.Register(Mock.Of<UserModel>(lm => lm.Username == "User")).Result;
            Assert.IsTrue(result is NotFoundObjectResult);
        }

        [TestMethod]
        public void TestGetNews()
        {
            var news = new List<string> { "news1", "news2" };
            newsService = GetNewsService(news);
            var mainController = GetMainController();

            var result = mainController.GetNews(string.Empty).Result;
            Assert.AreEqual(news, result);
        }

        [TestMethod]
        public void TestGetNewsNoService()
        {
            newsService = GetEmptyNewsService();
            var mainController = GetMainController();

            var result = mainController.GetNews(string.Empty).Result;
            Assert.IsTrue(result == null);
        }

        [TestMethod]
        public void TestGetNewsNoUser()
        {
            accountsService = GetAccountsService(HttpStatusCode.BadRequest);
            var mainController = GetMainController();

            var result = mainController.GetNews(string.Empty).Result;
            Assert.IsTrue(result == null);
        }

        [TestMethod]
        public void TestAddNewsValid()
        {
            newsService = GetNewsService(addNewsCode: HttpStatusCode.OK);
            var mainController = GetMainController();

            var result = mainController.AddNews(Mock.Of<NewsModel>()).Result;
            Assert.IsTrue(result is OkResult);
        }

        [TestMethod]
        public void TestAddNewsNotValid()
        {
            newsService = GetNewsService(addNewsCode: HttpStatusCode.BadRequest);
            var mainController = GetMainController();

            var result = mainController.AddNews(Mock.Of<NewsModel>()).Result;
            Assert.IsTrue(result is BadRequestObjectResult);
        }

        [TestMethod]
        public void TestAddNewsNoService()
        {
            newsService = GetEmptyNewsService();
            var mainController = GetMainController();

            var result = mainController.AddNews(Mock.Of<NewsModel>()).Result;
            Assert.IsTrue(result is NotFoundObjectResult);
        }

        [TestMethod]
        public void TestAddNewsNoUser()
        {
            accountsService = GetAccountsService(HttpStatusCode.BadRequest);
            var mainController = GetMainController();

            var result = mainController.AddNews(Mock.Of<NewsModel>()).Result;
            Assert.IsTrue(result is NotFoundObjectResult);
        }

        [TestMethod]
        public void TestAddNewsNoAccountsService()
        {
            accountsService = GetEmptyAccountsService();
            var mainController = GetMainController();

            var result = mainController.AddNews(Mock.Of<NewsModel>()).Result;
            Assert.IsTrue(result is NotFoundObjectResult);
        }

        [TestMethod]
        public void TestGetSubscribedAuthors()
        {
            var authors = new List<string> { "author1", "author2" };
            subscriptionsService = GetSubscriptionsService(authors: authors);
            var mainController = GetMainController();

            var result = mainController.GetSubscribedAuthors(string.Empty).Result;
            Assert.AreEqual(result, authors);
        }

        [TestMethod]
        public void TestGetSubscribedAuthorsNoService()
        {
            subscriptionsService = GetEmptySubscriptionsService();
            var mainController = GetMainController();

            var result = mainController.GetSubscribedAuthors(string.Empty).Result;
            Assert.IsTrue(result == null);
        }

        [TestMethod]
        public void TestAddSubscriptionValid()
        {
            subscriptionsService = GetSubscriptionsService(addCode: HttpStatusCode.OK);
            var mainController = GetMainController();

            var result = mainController.AddSubscription(string.Empty, string.Empty).Result;
            Assert.IsTrue(result is OkResult);
        }

        [TestMethod]
        public void TestAddSubscriptionNotValid()
        {
            subscriptionsService = GetSubscriptionsService(addCode: HttpStatusCode.InternalServerError);
            var mainController = GetMainController();

            var result = mainController.AddSubscription(string.Empty, string.Empty).Result;
            Assert.IsTrue(result is BadRequestObjectResult);
        }

        [TestMethod]
        public void TestAddSubscriptionNoService()
        {
            subscriptionsService = GetEmptySubscriptionsService();
            var mainController = GetMainController();

            var result = mainController.AddSubscription(string.Empty, string.Empty).Result;
            Assert.IsTrue(result is NotFoundObjectResult);
        }

        [TestMethod]
        public void TestRemoveSubscriptionValid()
        {
            subscriptionsService = GetSubscriptionsService(removeCode: HttpStatusCode.OK);
            var mainController = GetMainController();

            var result = mainController.RemoveSubscription(string.Empty, string.Empty).Result;
            Assert.IsTrue(result is OkResult);
        }

        [TestMethod]
        public void TestRemoveSubscriptionNotValid()
        {
            subscriptionsService = GetSubscriptionsService(removeCode: HttpStatusCode.InternalServerError);
            var mainController = GetMainController();

            var result = mainController.RemoveSubscription(string.Empty, string.Empty).Result;
            Assert.IsTrue(result is BadRequestObjectResult);
        }

        [TestMethod]
        public void TestRemoveSubscriptionNoService()
        {
            subscriptionsService = GetEmptySubscriptionsService();
            var mainController = GetMainController();

            var result = mainController.RemoveSubscription(string.Empty, string.Empty).Result;
            Assert.IsTrue(result is NotFoundObjectResult);
        }

        [TestMethod]
        public void TestAddSubscriptionNoUser()
        {
            accountsService = GetAccountsService(loginCode: HttpStatusCode.BadRequest);
            var mainController = GetMainController();

            var result = mainController.AddSubscription(string.Empty, string.Empty).Result;
            Assert.IsTrue(result is NotFoundObjectResult);
        }

        [TestMethod]
        public void TestAddSubscriptionNoAccountsService()
        {
            accountsService = GetEmptyAccountsService();
            var mainController = GetMainController();

            var result = mainController.AddSubscription(string.Empty, string.Empty).Result;
            Assert.IsTrue(result is NotFoundObjectResult);
        }

        [TestMethod]
        public void TestRemoveSubscriptionNoUser()
        {
            accountsService = GetAccountsService(loginCode: HttpStatusCode.BadRequest);
            var mainController = GetMainController();

            var result = mainController.RemoveSubscription(string.Empty, string.Empty).Result;
            Assert.IsTrue(result is NotFoundObjectResult);
        }

        [TestMethod]
        public void TestRemoveSubscriptionNoAccountsService()
        {
            accountsService = GetEmptyAccountsService();
            var mainController = GetMainController();

            var result = mainController.RemoveSubscription(string.Empty, string.Empty).Result;
            Assert.IsTrue(result is NotFoundObjectResult);
        }

        #region Support
        private ISubscriptionsService GetSubscriptionsService(List<string> authors = null, HttpStatusCode addCode = HttpStatusCode.OK, HttpStatusCode removeCode = HttpStatusCode.OK)
        {
            return Mock.Of<ISubscriptionsService>(srv =>
                //srv.GetSubscribedAuthorsForName(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()) == Task.FromResult(authors) &&
                srv.AddSubscription(It.IsAny<string>(), It.IsAny<string>()) == Task.FromResult(GetResponseMessage(addCode)) &&
                srv.RemoveSubscription(It.IsAny<string>(), It.IsAny<string>()) == Task.FromResult(GetResponseMessage(removeCode)));
        }

        private IAccountsService GetAccountsService(HttpStatusCode loginCode = HttpStatusCode.OK,
            HttpStatusCode registerCode = HttpStatusCode.OK,
            string currentUsername = "User",
            HttpStatusCode removeClaimCode = HttpStatusCode.OK)
        {
            return Mock.Of<IAccountsService>(srv =>
                srv.CheckIfUserExists(It.IsAny<UserModel>()) == Task.FromResult(GetResponseMessage(loginCode)) &&
                srv.Register(It.IsAny<UserModel>()) == Task.FromResult(GetResponseMessage(registerCode)));
        }

        private INewsService GetNewsService(List<string> getNewsContent = null, HttpStatusCode addNewsCode = HttpStatusCode.OK)
        {
            return Mock.Of<INewsService>(srv =>
                srv.AddNews(It.IsAny<NewsModel>()) == Task.FromResult(GetResponseMessage(addNewsCode)));
        }

        private IAccountsService GetEmptyAccountsService()
        {
            return Mock.Of<IAccountsService>();
        }

        private INewsService GetEmptyNewsService()
        {
            return Mock.Of<INewsService>();
        }

        private ISubscriptionsService GetEmptySubscriptionsService()
        {
            return Mock.Of<ISubscriptionsService>();
        }

        private GatewayController GetMainController()
        {
            return new GatewayController(logger, accountsService, subscriptionsService, newsService);
        }

        private HttpResponseMessage GetResponseMessage(HttpStatusCode registerCode)
        {
            return Mock.Of<HttpResponseMessage>(hwr => hwr.StatusCode == registerCode);
        }
        #endregion
    }
}
