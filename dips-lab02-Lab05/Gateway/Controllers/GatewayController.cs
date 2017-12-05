using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Gateway.Models;
using Gateway.Services;
using Microsoft.Extensions.Logging;
using Gateway.Services.Implementations;
using Gateway.Scheduling;
using System.Text.RegularExpressions;
using Gateway.Pagination;

namespace Gateway.Controllers
{
    [Route("api")]
    [Authorize]
    public class GatewayController : Controller
    {
        private IAccountsService accountsService;
        private ISubscriptionsService subscriptionsService;
        private INewsService newsService;
        private ILogger<GatewayController> logger;

        public GatewayController(ILogger<GatewayController> logger,
            IAccountsService accountsService,
            ISubscriptionsService subscriptionsService,
            INewsService newsService)
        {
            this.logger = logger;
            this.accountsService = accountsService;
            this.subscriptionsService = subscriptionsService;
            this.newsService = newsService;
        }

        [HttpGet("user/{username}")]
        public async Task<ObjectResult> CheckIfUserExist(UserModel userModel)
        {
            var response = await accountsService.CheckIfUserExists(userModel);
            logger.LogInformation($"Response from accounts service: {response?.StatusCode}");
            if (response?.IsSuccessStatusCode == true)
                return Ok("");
            return StatusCode(500, response != null ? "User doesn't exist" : "Acconts service unavailable");
        }

        [HttpPost("user")]
        public async Task<ObjectResult> Register(UserModel userModel)
        {
            if (userModel.Username == null || !Regex.IsMatch(userModel.Username, @"\S+"))
                return BadRequest("Name not valid");

            var response = await accountsService.Register(userModel);
            logger.LogInformation($"Response from accounts service: {response?.StatusCode}");

            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return StatusCode(200, "");
            }
            else if (response != null)
            {
                string respContent = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                logger.LogError($"User {userModel.Username} not registered, error content: {respContent}");
                return StatusCode(500, respContent);
            }
            else
            {
                logger.LogCritical("Accounts service unavailable");
                return StatusCode(503, "Accounts service unavailable");
            }
        }

        // Модифицирует несколько сервисов
        [HttpDelete("user/{username}")]
        public async Task<ObjectResult> DeleteUser(string username)
        {
            var accountsServerResult = await accountsService.DeleteUser(username);
            if (accountsServerResult == null)
            {
                return StatusCode(503, "Accounts service unavailable");
            }
            else if (string.IsNullOrWhiteSpace(accountsServerResult.Username))
                return StatusCode(500, "User not found");

            var newsServerResult = await newsService.DeleteNewsWithAuthor(username);
            if (newsServerResult == null)
            {
                var result = (await accountsService.Register(accountsServerResult))?.IsSuccessStatusCode == true;
                return StatusCode(503, $"News service unavailable, rollback status: Acc - {(result ? "ok" : "failed")}");
            }

            var subscriptionsServerResult = await subscriptionsService.RemoveAllAssociatedSubscriptions(username);
            if (subscriptionsServerResult == null)
            {
                var resultNews = true;
                foreach (var news in newsServerResult)
                    resultNews &= (await newsService.AddNews(news))?.IsSuccessStatusCode == true;
                var resultAccounts = (await accountsService.Register(accountsServerResult))?.IsSuccessStatusCode == true;
                return StatusCode(503, $"Subscriptions service unavailable, rollback status: Acc - { (resultAccounts ? "ok" : "failed")}, News - {(resultNews ? "ok" : "failed")}");
            }

            return Ok("");
        }

        // Модифицирует несколько сервисов
        [HttpPost("user/{username}")]
        public async Task<ObjectResult> ChangeUserName(string username, string newUsername)
        {
            var accountsResult = await accountsService.ChangeUserName(username, newUsername);
            var newsResult = await newsService.ChangeUserName(username, newUsername);
            var subscriptionsResult = await subscriptionsService.ChangeUserName(username, newUsername);

            if (accountsResult == null || newsResult == null || subscriptionsResult == null)
            {
                if (accountsResult != null)
                    await accountsService.ChangeUserName(newUsername, username);
                if (newsResult != null)
                    await newsService.ChangeUserName(newUsername, username);
                if (subscriptionsResult != null)
                    await subscriptionsService.ChangeUserName(newUsername, username);

                Scheduler.ScheduleRetryUntilSuccess(async () =>
                {
                    using (var client = new HttpClient())
                    {
                        try
                        {
                            accountsResult = await accountsService.ChangeUserName(username, newUsername);
                            newsResult = await newsService.ChangeUserName(username, newUsername);
                            subscriptionsResult = await subscriptionsService.ChangeUserName(username, newUsername);

                            if (accountsResult == null || newsResult == null || subscriptionsResult == null)
                            {
                                if (accountsResult != null)
                                    await accountsService.ChangeUserName(newUsername, username);
                                if (newsResult != null)
                                    await newsService.ChangeUserName(newUsername, username);
                                if (subscriptionsResult != null)
                                    await subscriptionsService.ChangeUserName(newUsername, username);
                                return false;
                            }
                            return true;
                        }
                        catch { }
                        return false;
                    }
                });
                return StatusCode(200, "Services status: " +
                    $"AS: {(accountsResult != null ? "online" : "offline")};" +
                    $"NS: {(newsResult != null ? "online" : "offline")};" +
                    $"SS: {(subscriptionsResult != null ? "online" : "offline")};");
            }

            var message = string.Empty;
            if (!accountsResult.IsSuccessStatusCode)
                message += $"AS operation status: {accountsResult.StatusCode}, {accountsResult.Content.ReadAsStringAsync().Result};";
            if (!newsResult.IsSuccessStatusCode)
                message += $"NS operation status: {newsResult.StatusCode}, {newsResult.Content.ReadAsStringAsync().Result};";
            if (!subscriptionsResult.IsSuccessStatusCode)
                message += $"SS operation status: {subscriptionsResult.StatusCode}, {subscriptionsResult.Content.ReadAsStringAsync().Result};";

            if (string.IsNullOrWhiteSpace(message))
                return Ok("");
            return StatusCode(500, message);
        }

        // Запрашивает информацию с нескольких сервисов
        [HttpGet("{name}/news")]
        public async Task<ObjectResult> GetNews(string name, int page = 0, int perpage = 0)
        {
            var message = string.Empty;
            var userExists = await accountsService.CheckIfUserExists(new UserModel { Username = name });
            int maxPage = 0;

            if (userExists == null)
                message = "Accounts service unavailable";
            else if (!userExists.IsSuccessStatusCode)
                message = $"User doesn't exist";
            else
            {
                List<string> authors = (await subscriptionsService.GetSubscribedAuthorsForName(name, 0, 0))?.Content;
                IEnumerable<string> news = Enumerable.Empty<string>();
                if (authors == null)
                {
                    logger.LogCritical("Subscriptions service unavailable");
                }
                else
                {
                    authors = authors.Concat(new[] { name }).ToList();
                    foreach (var author in authors)
                    {
                        PaginatedList<string> paginatedList = (await newsService.GetNewsByUser(author, page, perpage));
                        if (paginatedList != null && paginatedList.Content != null && paginatedList.Content.Count > 0)
                        {
                            news = news.Concat(paginatedList.Content ?? Enumerable.Empty<string>());
                            perpage = Math.Max(0, perpage - paginatedList.Content.Count);
                            maxPage += paginatedList.MaxPage;
                        }
                    }
                }

                if (news != null && news.Count() > 0)
                {
                    return StatusCode(200, new PaginatedList<string>(news.ToList(), perpage, page, maxPage));
                }
                else if (news != null)
                {
                    return StatusCode(200, "");
                }
                else
                    message = "News service unavailable";
            }
            logger.LogCritical(message);
            return StatusCode(500, message);
        }

        [HttpPost("news")]
        public async Task<ObjectResult> AddNews([Bind(new[] { "Header", "Body", "Author" })]NewsModel news)
        {
            if (string.IsNullOrWhiteSpace(news.Author) ||
                string.IsNullOrWhiteSpace(news.Body) ||
                string.IsNullOrWhiteSpace(news.Header))
                return StatusCode(400, "News model not fulfilled");

            var authorExists = await accountsService.CheckIfUserExists(new UserModel { Username = news.Author });
            logger.LogInformation($"Author response: {authorExists?.StatusCode}");
            if (authorExists == null)
                return StatusCode(503, "Accounts service unavailable");
            if (authorExists.StatusCode != System.Net.HttpStatusCode.OK)
                return StatusCode(500, "Author doesn't exists");

            var response = await newsService.AddNews(news);
            if (response != null)
            {
                logger.LogInformation($"Attempt to add news, response {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                    return Ok("");
                else
                    return StatusCode(500, response.Content?.ReadAsStringAsync()?.Result);
            }
            else
            {
                logger.LogCritical("News service unavailable");
                return StatusCode(503, "News service unavailable");
            }
        }

        [HttpGet("{subscriber}/subscriptions")]
        public async Task<ObjectResult> GetSubscribedAuthors(string subscriber, int page = 0, int perpage = 0)
        {
            var subscriberExists = await accountsService.CheckIfUserExists(new UserModel { Username = subscriber });

            logger.LogInformation($"Subscriber response: {subscriberExists?.StatusCode}");

            if (subscriberExists == null)
                return StatusCode(503, "Accounts service unavailable");
            if (subscriberExists.StatusCode != System.Net.HttpStatusCode.OK)
                return StatusCode(500, "User doesn't exist");

            logger.LogInformation($"Requesting authors for user {subscriber}");
            var authors = (await subscriptionsService.GetSubscribedAuthorsForName(subscriber, page, perpage));
            if (authors != null)
            {
                logger.LogInformation($"Found {authors.Content.Count()} authors for user {subscriber}");
                return StatusCode(200, authors);
            }
            else
            {
                logger.LogCritical("Subscriptions service unavailable");
                return StatusCode(503, "Subscriptions service unavailable");
            }
        }

        [HttpPost("{subscriber}/subscriptions/{author}")]
        public async Task<ObjectResult> AddSubscription(string subscriber, string author)
        {
            var subscriberExists = await accountsService.CheckIfUserExists(new UserModel { Username = subscriber });
            var authorExists = await accountsService.CheckIfUserExists(new UserModel { Username = author });
            logger.LogInformation($"Subscriber response: {subscriberExists?.StatusCode}, author response: {authorExists?.StatusCode}");

            if (subscriberExists == null || authorExists == null)
                return StatusCode(503, "Accounts service unavailable");
            if (subscriberExists.StatusCode != System.Net.HttpStatusCode.OK)
                return StatusCode(500, "Subscriber doesn't exists");
            if (authorExists.StatusCode != System.Net.HttpStatusCode.OK)
                return StatusCode(500, "Author doesn't exists");

            var response = await subscriptionsService.AddSubscription(subscriber, author);
            if (response != null)
            {
                logger.LogInformation($"Attempt to add subscription {subscriber}-{author}, response {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                    return Ok("");
                else
                    return StatusCode(500, response.Content?.ReadAsStringAsync()?.Result);
            }
            else
            {
                logger.LogCritical("Subscriptions service unavailable");
                return StatusCode(503, "Service unavailable");
            }
        }

        [HttpDelete("{subscriber}/subscriptions/{author}")]
        public async Task<ObjectResult> RemoveSubscription(string subscriber, string author)
        {
            var subscriberExists = await accountsService.CheckIfUserExists(new UserModel { Username = subscriber });
            var authorExists = await accountsService.CheckIfUserExists(new UserModel { Username = author });

            logger.LogInformation($"Subscriber response: {subscriberExists?.StatusCode}, author response: {authorExists?.StatusCode}");

            if (subscriberExists == null || authorExists == null)
                return StatusCode(503, "Accounts service unavailable");
            if (subscriberExists.StatusCode != System.Net.HttpStatusCode.OK)
                return StatusCode(500, "Subscriber doesn't exists");
            if (authorExists.StatusCode != System.Net.HttpStatusCode.OK)
                return StatusCode(500, "Author doesn't exists");

            var response = await subscriptionsService.RemoveSubscription(subscriber, author);
            if (response != null)
            {
                logger.LogInformation($"Attempt to remove subscription {subscriber}-{author}, response {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                    return Ok("");
                else
                    return StatusCode(500, response.Content?.ReadAsStringAsync()?.Result);
            }
            else
            {
                logger.LogCritical("Subscriptions service unavailable");
                return StatusCode(503, "Service unavailable");
            }
        }

        [HttpPost("auth/login")]
        public async Task<ObjectResult> Login(UserModel userModel)
        {
            var result = await accountsService.Login(userModel);
            if (result == null)
                return StatusCode(503, "Accounts service unavailable");
            else if (result.IsSuccessStatusCode)
                return Ok("");
            else
                return StatusCode(500, result.Content.ReadAsStringAsync().Result);
        }
    }
}
