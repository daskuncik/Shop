using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Gateway.Models.Shared;
using System.Text.RegularExpressions;
using Gateway.Models.Users;
using Gateway.CustomAuthorization;

namespace Gateway.Controllers
{
    [Route("users")]
    public class UsersController : Controller
    {
        private GatewayController gatewayController;
        private TokensStore tokenStore;

        public UsersController(GatewayController gatewayController, TokensStore tokenStore)
        {
            this.gatewayController = gatewayController;
            this.tokenStore = tokenStore;
        }
        public async Task<IActionResult> Index(IndexModel indexModel)
        {
            if (Request.Headers.Keys.Contains(CustomAuthorizationMiddleware.UserWord))
                indexModel.Username = string.Join(string.Empty, Request.Headers[CustomAuthorizationMiddleware.UserWord]);
            if (!string.IsNullOrWhiteSpace(indexModel.Username))
                return View(indexModel);
            else
                return RedirectToAction(nameof(Authenticate));
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.Keys.Contains(CustomAuthorizationMiddleware.AuthorizationWord))
            {
                var cookie = Request.Cookies[CustomAuthorizationMiddleware.AuthorizationWord];
                Response.Cookies.Append(CustomAuthorizationMiddleware.AuthorizationWord, cookie, new Microsoft.AspNetCore.Http.CookieOptions { Expires = DateTime.Now.AddDays(-1) });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("auth")]
        public async Task<IActionResult> Authenticate(AuthenticationModel authenticationModel)
        {
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AuthenticationModel authenticationModel)
        {
            var result = await gatewayController.Register(new Models.UserModel { Username = authenticationModel.Username, Password = authenticationModel.Password });
            if (result.StatusCode != 200)
                return View("Error", new ErrorModel(result));
            return await Login(authenticationModel);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthenticationModel authenticationModel)
        {
            var result = await gatewayController.Login(new Models.UserModel { Username = authenticationModel.Username, Password = authenticationModel.Password });
            if (result.StatusCode != 200)
                return View("Error", new ErrorModel(result));
            else
            {
                var token = tokenStore.GetToken(authenticationModel.Username, TimeSpan.FromMinutes(10));
                Response.Cookies.Append(CustomAuthorizationMiddleware.AuthorizationWord, $"Bearer {token}");
                if (authenticationModel.Redirect != null)
                    return Redirect(authenticationModel.Redirect);
                return RedirectToAction(nameof(Index), new IndexModel ());
            }
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(string username, bool submit = false)
        {
            if (submit)
            {
                var result = await gatewayController.DeleteUser(username);
                if (result.StatusCode == 200)
                    return await Logout();
                return View("Error", new ErrorModel(result));
            }
            return View(nameof(Delete), username);
        }

        [HttpGet("change")]
        public async Task<IActionResult> Change(string username, string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
            {
                return View(nameof(Change), username);
            }
            var result = await gatewayController.ChangeUserName(username, newUsername);
            if (result.StatusCode == 200)
                return await Logout();
            return View("Error", new ErrorModel(result));
        }
    }
}