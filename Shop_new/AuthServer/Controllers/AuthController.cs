using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AuthServer.Models;

namespace AuthServer.Controllers
{
    [Route("account")]
    public class AuthController : Controller
    {
        private AppDbContext dbContext;

        public AuthController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            return await Login(new LoginModel { Username = "User1", Password = "pass1", ReturnUrl = returnUrl });
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

    }
}
