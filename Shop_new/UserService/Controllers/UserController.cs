using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserService.Models;

namespace UserService.Controllers
{
    [Route("api")]
    public class UserController : Controller
    {
        // GET api/values
        UserDbContext db;

        [HttpGet("check")]
        public async Task<IActionResult> Check()
        {
            return Ok();
        }

        [HttpPost("user")]
        public async Task<IActionResult> Register(string name)
        {
            User user = db.Users.FirstOrDefault(u => u.Name == name);
            if (user == null)
            {
                User us = new Models.User
                {
                    Name = name
                };
                var result = db.Users.Add(us);
                return Ok();
            }
            return BadRequest("Duplicate");
        }

        [HttpPost("exists")]
        public async Task<IActionResult> CheckIfUserExists(string name)
        {
            User user = db.Users.FirstOrDefault(u => u.Name == name);
            if (user != null)
                return Ok();

            return Unauthorized();
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var user = db.Users.FirstOrDefault(u => u.Name == username);
            if (user != null)
            {
                var result = db.Users.Remove(user);
                db.SaveChanges();
                return Ok();
            }
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
                    db.SaveChanges();
                }
                else
                    message = $"User with username {newUsername} already exists";
            }
            else
                message = $"User with username {username} not found";

            if (string.IsNullOrWhiteSpace(message))
                return Ok();
            return StatusCode(500, message);
        }
    }
}
