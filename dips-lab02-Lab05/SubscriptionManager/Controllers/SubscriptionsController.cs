using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gateway.Pagination;

namespace SubscriptionManager.Controllers
{
    [Route("")]
    public class SubscriptionsController : Controller
    {
        private ApplicationDbContext db;
        private ILogger<SubscriptionsController> logger;

        public SubscriptionsController(ApplicationDbContext db, ILogger<SubscriptionsController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check()
        {
            return Ok();
        }

        [HttpGet("{subscriber}")]
        public async Task<PaginatedList<string>> GetAuthorsForName(string subscriber, int page, int perpage)
        {
            int maxPage = 0;
            logger.LogDebug($"Retriving subscriptions for user {subscriber}");
            var result = db.Subscriptions.Where(s => s.Subscriber == subscriber);
            logger.LogDebug($"Found {result.Count()} authors for user {subscriber}");
            if (perpage != 0)
                maxPage = result.Count() / perpage + (result.Count() % perpage == 0 ? -1 : 0) ;
            if (page != 0 && perpage != 0)
            {
                logger.LogDebug($"Skipping {page * perpage} entities due to pagination");
                result = result.Skip(page * perpage);
            }
            if (perpage != 0)
            {
                logger.LogDebug($"Taking at max {perpage} entities");
                result = result.Take(perpage);
            }
            logger.LogDebug($"Retrieved {result.Count()} authors: {string.Join(", ", result)}");
            return new PaginatedList<string>(result.Select(s => s.Author).ToList(), perpage, page, maxPage);
        }

        [HttpPost("{subscriber}")]
        public async Task<IActionResult> AddSubscription(string subscriber, string author)
        {
            var prevSubscription = db.Subscriptions.FirstOrDefault(s => s.Subscriber == subscriber && s.Author == author);
            if (prevSubscription == null)
            {
                logger.LogDebug($"Adding subscription: {subscriber} subscribes on feed of {author}");
                var subscription = new Subscription { Subscriber = subscriber, Author = author };
                var state = db.Subscriptions.Add(subscription)?.State;
                logger.LogDebug($"Subscription {subscriber}-{author} added with state {state}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"Subscription {subscriber}-{author} already exists");
            return BadRequest("Duplicate");
        }

        [HttpDelete("{subscriber}/{author}")]
        public async Task<IActionResult> RemoveSubscription(string subscriber, string author)
        {
            logger.LogDebug($"Removing subscription {subscriber}-{author}");
            var subscription = db.Subscriptions.FirstOrDefault(s => s.Subscriber == subscriber && s.Author == author);
            if (subscription != null)
            {
                logger.LogDebug($"Subscription {subscriber}-{author} exists in database");
                var state = db.Subscriptions.Remove(subscription)?.State;
                logger.LogDebug($"Subscription {subscriber}-{author} removed with state {state}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"Subscription {subscriber}-{author} not found");
            return BadRequest();
        }

        [HttpGet("all/{name}")]
        public async Task<List<string>> GetAllAssociatedSubscriptions(string name, int page, int perpage)
        {
            var subscriptions = db.Subscriptions.Where(s => s.Subscriber == name || s.Author == name);
            logger.LogDebug($"Found {subscriptions.Count()} associated with user {name}");
            subscriptions = subscriptions.OrderBy(s => s.Author);
            if (page != 0 && perpage != 0)
            {
                logger.LogDebug($"Skipping {page * perpage} entities due to pagination");
                subscriptions = subscriptions.Skip(page * perpage);
            }
            if (perpage != 0)
            {
                logger.LogDebug($"Taking at max {perpage} entities");
                subscriptions = subscriptions.Take(perpage);
            }
            logger.LogDebug($"Retrieved {subscriptions.Count()}");
            return subscriptions.Select(s => $"{s.Subscriber} subscribed to {s.Author}").ToList();
        }

        [HttpDelete("all/{name}")]
        public async Task<IActionResult> RemoveAllAssociatedSubscriptions(string name)
        {
            var subscriptions = db.Subscriptions.Where(s => s.Author == name || s.Subscriber == name);
            logger.LogDebug($"Found {subscriptions.Count()} associated with user {name}");
            if (subscriptions.Count() > 0)
            {
                db.Subscriptions.RemoveRange(subscriptions);
                db.SaveChanges();
            }
            return Ok(subscriptions.Select(s => new Tuple<string, string>(s.Author, s.Subscriber)).ToList());
        }

        [HttpPut("user/{username}")]
        public async Task<IActionResult> ChangeUserName(string username, string newUsername)
        {
            string message = string.Empty;
            var subscriptions = db.Subscriptions.Where(s => s.Author == username || s.Subscriber == username);
            if (subscriptions.Any())
            {
                var otherUserSubscriptions = db.Subscriptions.Where(s => s.Author == newUsername || s.Subscriber == newUsername);
                if (!otherUserSubscriptions.Any())
                {
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.Author == username)
                            subscription.Author = newUsername;
                        if (subscription.Subscriber == username)
                            subscription.Subscriber = newUsername;
                    }

                    db.Subscriptions.UpdateRange(subscriptions);
                    db.SaveChanges();
                }
                else
                    message = $"Subscriptions for/by user with username {newUsername} already exists";
            }

            logger.LogDebug(message);
            if (string.IsNullOrWhiteSpace(message))
                return Ok();
            return StatusCode(500, message);
        }
    }
}
