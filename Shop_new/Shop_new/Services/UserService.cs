using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Shop_new.Models;

namespace Shop_new.Services
{
    public class UserService : Service
    {
        public UserService(IConfiguration conf) : base(conf.GetSection("Addresses")["Users"]) { }

        public async Task<HttpResponseMessage> Register(string name, string password)
        {
            try
            {
                return await PostForm("user", new Dictionary<string, string>() { {"name", name }, {"password", password } });

            }
            catch
            {
                return null;
            }

        }

        public async Task<HttpResponseMessage> CheckIfUserExists(string name)
        {
            try
            {
                return await PostForm("exists", new Dictionary<string, string>() { { "name", name } });

            }
            catch
            {
                return null;
            }

        }

        public async Task<HttpResponseMessage> RemoveUser(string name)
        {
            try
            {
                return await Delete($"{name}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> ChangeUserName(string name)
        {
            try
            {
                return await PostForm($"user/{name}", new Dictionary<string, string>());

            }
            catch
            {
                return null;
            }

        }

        public async Task<HttpResponseMessage> Login(string username, string password) //=> await PostJson("login", userModel);
        {
            try
            {
                return await PostForm("login", new Dictionary<string, string>() { { "username", username }, { "password", password } });

            }
            catch
            {
                return null;
            }
        }

        public async Task<int> GetUserId(string name, string password)
        {
            var httpResponseMessage = await PostForm("getId", new Dictionary<string, string>() { { "username", name }, { "password", password } });
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return 0;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            int aa = Convert.ToInt32(response);
            return aa;

        }

        public async Task<int> GetUserIdByName(string name)
        {
            var httpResponseMessage = await PostForm("getId2", new Dictionary<string, string>() { { "username", name } });
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return 0;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            int aa = Convert.ToInt32(response);
            return aa;

        }
    }
}
