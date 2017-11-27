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

        public async Task<HttpResponseMessage> Register(string name)
        {
            try
            {
                return await PostForm("user", new Dictionary<string, string>() { {"name", name } });

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
    }
}
