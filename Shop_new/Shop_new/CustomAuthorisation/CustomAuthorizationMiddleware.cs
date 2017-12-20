using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shop_new.Services;
using System.Security.Claims;

namespace Shop_new.CustomAuthorisation
{
    public abstract class CustomAuthorizationMiddleware
    {
        public static string AuthorizationWord = "Authorization";
        public static string UserWord = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        public static string RoleWord = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        protected readonly RequestDelegate _next;
        //private AuthService auth;
        //protected readonly TokenStore tokensStore;
        

        public CustomAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
           // this.tokensStore = tokensStore;
        }

        public virtual async Task Invoke(HttpContext context)
        {
            if (context.Request.Cookies.Keys.Contains(AuthorizationWord))
            {
                var auth = context.Request.Cookies[AuthorizationWord];
                await CheckBearerAuthorization(context, auth);
            }
            else if (context.Request.Path.Value.Split('/').Intersect(GetAnonymousPaths()).Any())
            {
                await this._next(context);
            }
            else
            {
                var message = "No authorization header provided";
                await ReturnForbidden(context, message);
            }
        }

        protected async Task CheckBearerAuthorization(HttpContext context, string auth)
        {
            var match = Regex.Match(auth, @"Bearer (\S+)");
            if (match.Groups.Count == 1)
            {
                await ReturnForbidden(context, "Invalid token format");
            }
            else
            {
                var token = match.Groups[1].Value;
                var result = GetUserByToken(token);
                //var result = tokensStore.CheckToken(token);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    string aa = GetRole(result);
                    ClaimsIdentity identity = new ClaimsIdentity(new List<Claim>
                    {
                        new Claim(UserWord, result),
                        new Claim(RoleWord, aa)
                    }, "CustomAuthenticationType");
                    context.User.AddIdentity(identity);
                    context.Request.Headers.Add(UserWord, result);
                    await this._next(context);
                    // string name = await this.auth.GetTokenByName(token);
                    //context.Request.Headers.Add(UserWord, name);
                    // await this._next(context);
                }
                else
                    await ReturnForbidden(context, "Token not valid");
            }
        }

        public abstract Task ReturnForbidden(HttpContext context, string message);

        public abstract List<string> GetAnonymousPaths();

        public abstract string GetUserByToken(string token);

        public abstract string GetRole(string username);
    }
}
