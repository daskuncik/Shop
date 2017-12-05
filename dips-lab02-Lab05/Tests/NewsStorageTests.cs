using Gateway.Models;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NewsStorage;
using NewsStorage.Controllers;
using NewsStorage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class NewsStorageTests
    {
        private const string username = "User";
        private const string header = "Header";

        private ILogger<NewsController> logger;
        private ISubscriptionsService subscriptionsService;
        private ApplicationDbContext dbContext;

        [TestInitialize]
        public void TestInitialize()
        {
            logger = Mock.Of<ILogger<NewsController>>();
            subscriptionsService = GetSubscriptionsService();
            dbContext = GetDbContext();
        }

        [TestMethod]
        public void TestAddNewsValid()
        {
            var news = new List<News>();
            dbContext = GetDbContext(news);
            var newsController = GetNewsController();

            var result = newsController.AddNews(Mock.Of<NewsModel>(nm => nm.Author == username)).Result;
            Assert.IsTrue(result is OkResult);
            Assert.IsTrue(news.First().Author == username);
        }

        [TestMethod]
        public void TestAddNewsNotValid()
        {
            var news = new List<News> { new News { Author = username, Header = header } };
            dbContext = GetDbContext(news);
            var newsController = GetNewsController();

            var result = newsController.AddNews(Mock.Of<NewsModel>(nm => nm.Author == username && nm.Header == header)).Result;
            Assert.IsFalse(result is OkResult);
        }

        #region Support
        private NewsController GetNewsController()
        {
            return new NewsController(dbContext, logger);
        }

        private ApplicationDbContext GetDbContext(List<News> news = null)
        {
            if (news == null)
                news = new List<News>();
            return Mock.Of<ApplicationDbContext>(db =>
                db.News == GetNews(news));
        }

        private DbSet<News> GetNews(List<News> news)
        {
            var newsQueryable = news.AsQueryable();
            var mockSet = new Mock<DbSet<News>>();
            var mockSetQueryable = mockSet.As<IQueryable<News>>();
            mockSetQueryable.Setup(m => m.Provider).Returns(newsQueryable.Provider);
            mockSetQueryable.Setup(m => m.Expression).Returns(newsQueryable.Expression);
            mockSetQueryable.Setup(m => m.ElementType).Returns(newsQueryable.ElementType);
            mockSetQueryable.Setup(m => m.GetEnumerator()).Returns(news.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<News>())).Callback<News>(u => news.Add(u));
            return mockSet.Object;
        }

        private ISubscriptionsService GetSubscriptionsService(List<string> authors = null, HttpStatusCode addCode = HttpStatusCode.OK, HttpStatusCode removeCode = HttpStatusCode.OK)
        {
            return Mock.Of<ISubscriptionsService>(srv =>
                //srv.GetSubscribedAuthorsForName(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()) == Task.FromResult(authors) &&
                srv.AddSubscription(It.IsAny<string>(), It.IsAny<string>()) == Task.FromResult(GetResponseMessage(addCode)) &&
                srv.RemoveSubscription(It.IsAny<string>(), It.IsAny<string>()) == Task.FromResult(GetResponseMessage(removeCode)));
        }

        private HttpResponseMessage GetResponseMessage(HttpStatusCode registerCode)
        {
            return Mock.Of<HttpResponseMessage>(hwr => hwr.StatusCode == registerCode);
        }
        #endregion
    }
}
