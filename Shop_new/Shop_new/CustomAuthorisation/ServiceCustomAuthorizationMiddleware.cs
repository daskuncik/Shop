using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using Shop_new.Services;

namespace Shop_new.CustomAuthorisation
{
    public class ServiceCustomAuthorizationMiddleware :CustomAuthorizationMiddleware
    {
        private const string serviceWord = "Service";
        private List<(string, string)> allowedApps = new List<(string, string)> { ("AppId", "AppSecret") };
        private TokensStore tokensStore;

        public ServiceCustomAuthorizationMiddleware(RequestDelegate next, TokensStore tokensStore) : base(next)
        {
            this.tokensStore = tokensStore;
        }

        public override async Task Invoke(HttpContext context)
        {
            if (IsBasicAuthorizationSuccess(context))
            {
                context.Response.StatusCode = 200;
                string token = tokensStore.GetToken(serviceWord, TimeSpan.FromMinutes(15));
                await context.Response.WriteAsync(token);
                return;
            }
            else if (IsBearerAuthorization(context))
            {
                var auth = context.Request.Headers[AuthorizationWord];
                await CheckBearerAuthorization(context, auth);
            }
            else
                await base.Invoke(context);
        }


        private static bool IsBearerAuthorization(HttpContext context)
        {
            return context.Request.Headers.Keys.Contains(AuthorizationWord);
        }


        public override List<string> GetAnonymousPaths()
        {
            //return new[] { "login" }.ToList();
            return new List<string>();
        }

        public override async Task ReturnForbidden(HttpContext context, string message)
        {
            using (var writer = new StreamWriter(context.Response.Body))
            {
                context.Response.StatusCode = 401;
                await writer.WriteAsync(message);
            }
        }

        //private bool RequestedToken(HttpContext context)
        //{
        //    if (context.Request.Headers.Keys.Contains(AuthorizationWord))
        //    {
        //        var match = Regex.Match(string.Join(string.Empty, context.Request.Headers[AuthorizationWord]), @"Basic (\S+)");
        //        if (match.Groups.Count > 1)
        //        {
        //            var appIdAndSecret = Encoding.UTF8.GetString(Convert.FromBase64String(match.Groups[1].Value)).Split(':');
        //            if (allowedApps.Contains((appIdAndSecret[0], appIdAndSecret[1])))
        //                return true;
        //        }
        //    }
        //    return false;
        //}

        private bool IsBasicAuthorizationSuccess(HttpContext context)
        {
            if (context.Request.Headers.Keys.Contains(AuthorizationWord))
            {
                string auth = string.Join(string.Empty, context.Request.Headers[AuthorizationWord]);
                var match = Regex.Match(auth, @"Basic (\S+)");
                if (match.Groups.Count > 1)
                {
                    byte[] appIdAndSecretBytes = Convert.FromBase64String(match.Groups[1].Value);
                    var appIdAndSecret = Encoding.UTF8.GetString(appIdAndSecretBytes).Split(':');
                    if (allowedApps.Contains((appIdAndSecret[0], appIdAndSecret[1])))
                        return true;
                }
            }
            return false;
        }

        public override string GetUserByToken(string token)
        {
            return tokensStore.GetNameByToken(token);
        }

        public override string GetRole(string username)
        {
            return "";
        }

    }
}
