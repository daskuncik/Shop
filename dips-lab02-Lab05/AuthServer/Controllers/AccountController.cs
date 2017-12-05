using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

using AuthServer.Models.Account;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Models;
using IdentityServer4;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AuthServer.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private ApplicationDbContext dbContext;

        public AccountController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        //[HttpGet]
        //[Route("login")]
        //public async Task<IActionResult> Login(string returnUrl)
        //{
        //    return View(new LoginModel { ReturnUrl = returnUrl });
        //}

        [HttpGet("login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            return await Login(new LoginModel { Username = "User1", Password = "pass1", ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("login")]
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
    }
}
