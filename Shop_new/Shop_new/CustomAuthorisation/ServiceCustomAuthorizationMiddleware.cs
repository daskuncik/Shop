using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace Shop_new.CustomAuthorisation
{
    public class ServiceCustomAuthorizationMiddleware :CustomAuthorizationMiddleware
    {
        private List<(string, string)> allowedApps = new List<(string, string)> { ("AppId", "AppSecret") };

        public ServiceCustomAuthorizationMiddleware(RequestDelegate next, TokenStore tokensStore) : base(next, tokensStore)
        {
        }

        public override async Task Invoke(HttpContext context)
        {
            if (RequestedToken(context))
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(tokensStore.GetToken(Guid.NewGuid().ToString(), TimeSpan.FromSeconds(5)));
                return;
            }
            else if (context.Request.Headers.Keys.Contains(AuthorizationWord))
            {
                var auth = context.Request.Headers[AuthorizationWord];
                await CheckAuthorization(context, auth);
            }
            else
                await base.Invoke(context);
        }

        public override List<string> GetAnonymousPaths()
        {
            return new[] { "login" }.ToList();
        }

        public override async Task ReturnForbidden(HttpContext context, string message)
        {
            using (var writer = new StreamWriter(context.Response.Body))
            {
                context.Response.StatusCode = 401;
                await writer.WriteAsync(message);
            }
        }

        private bool RequestedToken(HttpContext context)
        {
            if (context.Request.Headers.Keys.Contains(AuthorizationWord))
            {
                var match = Regex.Match(string.Join(string.Empty, context.Request.Headers[AuthorizationWord]), @"Basic (\S+)");
                if (match.Groups.Count > 1)
                {
                    var appIdAndSecret = Encoding.UTF8.GetString(Convert.FromBase64String(match.Groups[1].Value)).Split(':');
                    if (allowedApps.Contains((appIdAndSecret[0], appIdAndSecret[1])))
                        return true;
                }
            }
            return false;
        }

    }
}
