using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AuthServer.Models;
using AuthServer.Entities;
using AuthServer;
using Shop_new.Models;

namespace AuthServer.Controllers
{
    [Route("account")]
    public class AuthController : Controller
    {
        private UserContext userContext;
        private AppDbContext dbContext;
        private TokenDbContext tokenDbContext;

        public AuthController(AppDbContext dbContext, UserContext usrContext, TokenDbContext tknContext)
        {
            this.dbContext = dbContext;
            this.tokenDbContext = tknContext;
            this.userContext = usrContext;
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            //return await Login(new LoginModel { Username = "User1", Password = "pass1", ReturnUrl = returnUrl });
            return View(new LoginModel { ReturnUrl = returnUrl });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Username == loginModel.Username);
            if (user != null)
            {
                if (user.Password == loginModel.Password.Sha256())
                {
                    await AuthenticationManagerExtensions.SignInAsync(HttpContext, user.Id.ToString(), user.Username, new Claim("Name", loginModel.Username));
                    return Redirect(loginModel.ReturnUrl);
                }
            }
            return Redirect("~/");
        }


        [HttpPost("customlogin")]
        public async Task<IActionResult> CustomLogin(UserModel userModel)
        {
            var user = userContext.Users.FirstOrDefault(u => u.Username == userModel.UserName);
            if (user != null)
            {
                if (user.Password == userModel.Password.Sha256())
                {
                    var token = new Models.Token
                    {
                        Id = Guid.NewGuid().ToString(),
                        Expiration = DateTime.Now + TimeSpan.FromMinutes(30),
                        Owner = userModel.UserName
                    };
                    tokenDbContext.Add(token);
                    tokenDbContext.SaveChanges();
                    return Ok(token.Id);
                }
                else
                {
                    return StatusCode(500, "Wrong password");
                }
            }
            return StatusCode(500, "User not found");
        }

        [HttpGet("customlogout")]
        public async Task<IActionResult> CustomLogout(string token)
        {
            var tkn = tokenDbContext.Tokens.FirstOrDefault(x => x.Id == token);
            tokenDbContext.Remove(tkn);
            tokenDbContext.SaveChanges();
            return Ok();
        }


        [HttpPost("verify")]
        public async Task<IActionResult> VerifyToken(string token)
        {
            var tkn = tokenDbContext.Tokens.FirstOrDefault(x => x.Id == token);
            if (tkn != null)
            {
                if (tkn.Expiration > DateTime.Now)
                    return Ok(tkn.Owner);
                tokenDbContext.Remove(tkn);
                tokenDbContext.SaveChanges();
            }
            return Unauthorized();

            //var response = tokenStore.CheckToken(token);
            //if (response == CheckTokenResult.Valid)
            //    return Ok();

            //    return BadRequest();

        }


        //   Используется с TokenStore:


        [HttpPost("gettoken")]
        public async Task<string> GetToken(string _owner, TimeSpan expiration)
        {
            var a = new Models.Token
            {
                Id = Guid.NewGuid().ToString(),
                Owner = _owner,
                Expiration = DateTime.Now+expiration
            };
            tokenDbContext.Add(a);
            tokenDbContext.SaveChanges();
            //var response = tokenStore.GetToken(_owner, expiration);
            return a.Id;

        }

        [HttpPost("gettokenbyname")]
        public async Task<string> GetNameByToken(string token)
        {
            var a = tokenDbContext.Tokens.FirstOrDefault(q => q.Id == token).Id;
            return a;
            //var response = tokenStore.GetNameByToken(token);
            //return response;
           

        }

    }
}
