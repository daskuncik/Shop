using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Gateway.Models;
using Authentication.Entities;
using Microsoft.Extensions.Logging;
using Gateway.Extensions;
namespace Authentication.Controllers
{
    [Route("")]
    public class AccountsController : Controller
    {
        private ApplicationDbContext db;
        private ILogger<AccountsController> logger;

        public AccountsController(ApplicationDbContext db, ILogger<AccountsController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check()
        {
            return Ok();
        }

        [HttpPost("exists")]
        public async Task<IActionResult> CheckIfUserExists([FromBody] UserModel userModel)
        {
            User user = db.Users.FirstOrDefault(u => u.Name == userModel.Username);
            if (user != null)
            {
                logger.LogDebug($"User {user.Name} found");
                return Ok();
            }
            logger.LogWarning($"User {userModel.Username} not found");
            return Unauthorized();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserModel userModel)
        {
            logger.LogDebug($"Login request, username: {userModel.Username}");
            User user = db.Users.FirstOrDefault(u => u.Name == userModel.Username);
            if (user != null)
            {
                if (user.Password == userModel.Password.Sha256())
                {
                    logger.LogDebug($"User {user.Name} found");
                    return Ok();
                }
                else
                {
                    return StatusCode(500, "Wrong password");
                }
            }
            logger.LogWarning($"User {userModel.Username} not found");
            return StatusCode(500, "User not found");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserModel userModel)
        {
            logger.LogDebug($"Register request, username: {userModel.Username}");
            User user = db.Users.FirstOrDefault(u => u.Name == userModel.Username);
            if (user == null)
            {
                logger.LogDebug($"Registering user {userModel.Username}");
                var result = db.Users.Add(new User(userModel));
                logger.LogDebug($"User {userModel.Username} add result: {result?.State}");
                db.SaveChanges();
                return Ok();
            }
            logger.LogWarning($"User {userModel.Username} already exists");
            return BadRequest("Duplicate");
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var user = db.Users.FirstOrDefault(u => u.Name == username);
            if (user != null)
            {
                logger.LogDebug($"Removing user {username}");
                var result = db.Users.Remove(user);
                logger.LogDebug($"User {user.Name} removed result: {result?.State}");
                db.SaveChanges();
                return Ok(new UserModel { Username = user.Name });
            }
            logger.LogWarning($"User {username} not found");
            return NotFound();
        }

        [HttpPut("user/{username}")]
        public async Task<IActionResult> ChangeUserName(string username, string newUsername)
        {
            string message = string.Empty;

            var user = db.Users.FirstOrDefault(u => u.Name == username);
            if (user != null)
            {
                var otherUser = db.Users.FirstOrDefault(u => u.Name == newUsername);
                if (otherUser == null)
                {
                    user.Name = newUsername;
                    var result = db.Users.Update(user);
                    logger.LogDebug($"User {user.Name} update result: {result?.State}");
                    db.SaveChanges();
                }
                else
                    message = $"User with username {newUsername} already exists";
            }
            else
                message = $"User with username {username} not found";

            logger.LogDebug(message);
            if (string.IsNullOrWhiteSpace(message))
                return Ok();
            return StatusCode(500, message);
        }
    }
}
