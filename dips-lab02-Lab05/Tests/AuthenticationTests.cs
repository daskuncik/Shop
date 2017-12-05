using Authentication;
using Authentication.Controllers;
using Authentication.Entities;
using Gateway.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tests
{
    [TestClass]
    public class AuthenticationTests
    {
        private const string username = "User";
        private ApplicationDbContext dbContext;
        private ILogger<AccountsController> logger;

        [TestInitialize]
        public void TestInitialize()
        {
            logger = Mock.Of<ILogger<AccountsController>>();
            dbContext = Mock.Of<ApplicationDbContext>();
        }

        [TestMethod]
        public void TestLoginValid()
        {
            dbContext = GetDbContext(new List<User> { new User { Name = username } });
            var accountsController = GetAccountsController();

            var result = accountsController.CheckIfUserExists(Mock.Of<UserModel>(lm => lm.Username == username)).Result;
            Assert.IsTrue(result is OkResult);
        }

        [TestMethod]
        public void TestLoginNotValid()
        {
            dbContext = GetDbContext(new List<User> { new User { Name = username.Substring(1)} });
            var accountsController = GetAccountsController();

            var result = accountsController.CheckIfUserExists(Mock.Of<UserModel>(lm => lm.Username == username)).Result;
            Assert.IsFalse(result is OkResult);
        }

        [TestMethod]
        public void TestRegisterValid()
        {
            var users = new List<User>();
            dbContext = GetDbContext(users);
            var accountsController = GetAccountsController();

            var result = accountsController.Register(Mock.Of<UserModel>(um => um.Username == username )).Result;
            Assert.IsTrue(result is OkResult);
            Assert.IsTrue(users.Any(u => u.Name == username ));
        }

        [TestMethod]
        public void TestRegisterNotValid()
        {
            var users = new List<User> { new User { Name = username} };
            dbContext = GetDbContext(users);
            var accountsController = GetAccountsController();

            var result = accountsController.Register(Mock.Of<UserModel>(um => um.Username == username )).Result;
            Assert.IsFalse(result is OkResult);
        }

        #region Support
        private AccountsController GetAccountsController()
        {
            return new AccountsController(dbContext, logger);
        }

        private ApplicationDbContext GetDbContext(List<User> users = null)
        {
            if (users == null)
                users = new List<User>();
            return Mock.Of<ApplicationDbContext>(db =>
                db.Users == GetUsers(users));
        }

        private DbSet<User> GetUsers(List<User> users)
        {
            var usersQueryable = users.AsQueryable();
            var mockSet = new Mock<DbSet<User>>();
            var mockSetQueryable = mockSet.As<IQueryable<User>>();
            mockSetQueryable.Setup(m => m.Provider).Returns(usersQueryable.Provider);
            mockSetQueryable.Setup(m => m.Expression).Returns(usersQueryable.Expression);
            mockSetQueryable.Setup(m => m.ElementType).Returns(usersQueryable.ElementType);
            mockSetQueryable.Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<User>())).Callback<User>(u => users.Add(u));
            mockSet.Setup(d => d.Update(It.IsAny<User>())).Callback<User>(u =>
            {
                users = users.Where(u1 => u.Name != u1.Name).ToList();
                users.Add(u);
            });
            return mockSet.Object;
        }
        #endregion
    }
}