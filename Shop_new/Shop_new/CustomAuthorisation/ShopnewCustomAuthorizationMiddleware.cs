using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shop_new.Services;

namespace Shop_new.CustomAuthorisation
{
    public class ShopnewCustomAuthorizationMiddleware : CustomAuthorizationMiddleware
    {
        public override List<string> GetAnonymousPaths() => new[] { "api", "auth", "user", "login", "register" }.ToList();

        private AuthService authService;
 
        public ShopnewCustomAuthorizationMiddleware(RequestDelegate next, AuthService authService) : base(next)
        {
            this.authService = authService;
        }

        public override async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.Keys.Contains(AuthorizationWord))
            {
                await this._next(context);
            }
            else
                await base.Invoke(context);
        }

        public override async Task ReturnForbidden(HttpContext context, string message)
        {
            string redirect = "/user";
            if (context.Request.Path != redirect)
            {
                redirect += $"?Redirect={context.Request.Path}";
            }
            context.Response.Redirect(redirect);
        }

        public override string GetUserByToken(string token)
        {
            return authService.Verify(token)?.Result;
        }
    }
}
