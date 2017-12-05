using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsStorage.Entities;
using Microsoft.Extensions.Configuration;
using Gateway.Services;
using Gateway.Models;
using Microsoft.Extensions.Logging;
using Gateway.Pagination;

namespace NewsStorage.Controllers
{
    [Route("")]
    public class NewsController : Controller
    {
        private ApplicationDbContext db;
        private ILogger<NewsController> logger;

        public NewsController(ApplicationDbContext db,
            ILogger<NewsController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check()
        {
            return Ok();
        }

        [HttpPost("")]
        public async Task<IActionResult> AddNews([FromBody] NewsModel newsModel)
        {
            var prevNews = db.News.FirstOrDefault(n => n.Author == newsModel.Author && n.Header == newsModel.Header);
            if (prevNews == null)
            {
                logger.LogDebug($"Adding news, author: {newsModel.Author}");
                if (newsModel.Date == null)
                    newsModel.Date = DateTime.Now;
                var state = db.News.Add(new News(newsModel))?.State;
                logger.LogDebug($"News addition result: {state}");
                db.SaveChanges();
                return Ok();
            }
            else
            {
                logger.LogWarning($"News already exists");
                return StatusCode(500, "Duplicate");
            }
        }

        [HttpGet("author/{name}")]
        public async Task<PaginatedList<string>> GetNewsByUser(string name, int page, int perpage)
        {
            int maxPage = 0;
            logger.LogDebug($"Retrieving news by user {name}");
            var news = db.News.Where(n => n.Author == name);
            logger.LogDebug($"Found {news.Count()} news by user {name}");
            news = news.OrderByDescending(n => n.Date);
            if (perpage != 0)
                maxPage = news.Count() / perpage + (news.Count() % perpage == 0 ? -1 : 0);
            if (perpage != 0 && page != 0)
            {
                logger.LogDebug($"Skipping {perpage * page} news due to pagination");
                news = news.Skip(perpage * page);
            }
            if (perpage != 0)
            {
                logger.LogDebug($"Retrieving at max {perpage} news");
                news = news.Take(perpage);
            }
            logger.LogDebug($"Returning {news.Count()} news");
            return new PaginatedList<string>(news.Select(n => $"Header: {n.Header}{Environment.NewLine}Body: {n.Body}{Environment.NewLine}Author: {n.Author}")
                .ToList(), perpage, page, maxPage);
        }

        [HttpDelete("author/{name}")]
        public async Task<IActionResult> DeleteNewsByUser(string name)
        {
            var news = db.News.Where(n => n.Author == name);
            logger.LogDebug($"News by {name} count: {news.Count()}");
            if (news.Count() > 0)
            {
                db.News.RemoveRange(news);
                db.SaveChanges();
            }
            return Ok(news.Select(n => new NewsModel { Author = n.Author, Body = n.Body, Date = n.Date, Header = n.Header }).ToList());
        }

        [HttpPut("user/{username}")]
        public async Task<IActionResult> ChangeUserName(string username, string newUsername)
        {
            string message = string.Empty;

            var news = db.News.Where(n => n.Author == username);
            if (news.Any())
            {
                var otherUserNews = db.News.Where(n => n.Author == newUsername);
                if (!otherUserNews.Any())
                {
                    foreach (var singleNews in news)
                        singleNews.Author = newUsername;
                    db.News.UpdateRange(news);
                    db.SaveChanges();
                }
                else
                    message = $"News from author with username {newUsername} already exists";
            }

            logger.LogDebug(message);
            if (string.IsNullOrWhiteSpace(message))
                return Ok();
            return StatusCode(500, message);
        }
    }
}
