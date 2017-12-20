using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shop_new.Services;
using Statistics.EventBus;
using Statistics.Events;

namespace Shop_new.CustomAuthorisation
{
    public class ShopnewCustomAuthorizationMiddleware : CustomAuthorizationMiddleware
    {
        public override List<string> GetAnonymousPaths() => new[] { "api", "auth", "user", "login", "register" }.ToList();
        private IEventBus eventBus;
        private AuthService authService;
        private UserService usrService;
 
        public ShopnewCustomAuthorizationMiddleware(RequestDelegate next, AuthService authService, IEventBus bus, UserService usr) : base(next)
        {
            this.authService = authService;
            this.eventBus = bus;
            usrService = usr;
        }

        public override async Task Invoke(HttpContext context)
        {
            eventBus.Publish(new RequestEvent
            {
                Host = context.Connection.LocalIpAddress.ToString() + ":" + context.Connection.LocalPort.ToString(),
                Origin = context.Connection.RemoteIpAddress.ToString() + ":" + context.Connection.RemotePort.ToString(),
                Route = context.Request.Path.ToString(),
                RequestType = RequestType.Shop_new,
                OccurenceTime = DateTime.Now
            }, true);
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
        public override string GetRole(string username)
        {
            return usrService.GetRole(username)?.Result;
        }

    }
}
