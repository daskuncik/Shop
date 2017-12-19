using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shop_new.Services
{
    public class AuthService : Service
    {
        public AuthService(IConfiguration conf) : base(conf.GetSection("Addresses")["Auth"]) { }

        public async Task<string> Verify(string token)
        {
            var res = await PostForm("verify", new Dictionary<string, string>() { { "token", token } });
            try
            {
                return await res.Content.ReadAsStringAsync();

            }
            catch
            {
                return null;
            }

        }

        public async Task<string> GetToken(string _owner, TimeSpan expiration)
        {
            var httpResponseMessage = await PostForm("gettoken", new Dictionary<string, string>() { { "_owner",_owner }, { "expiration", expiration.ToString() } });
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                return response;
            }
            catch
            {
                return null;
            }

        }

        public async Task<string> GetTokenByName(string token)
        {
            var httpResponseMessage = await PostForm("gettokenbyname", new Dictionary<string, string>() { { "token", token } });
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                return response;
            }
            catch
            {
                return null;
            }

        }
    }
}
