using Gateway.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Services.Implementations
{
    public class AccountsService : Service, IAccountsService
    {
        public AccountsService(IConfiguration configuration) : 
            base(configuration.GetSection("Addresses")["Accs"]) { }

        public async Task<HttpResponseMessage> CheckIfUserExists(UserModel userModel) => await PostJson("exists", userModel);

        public async Task<HttpResponseMessage> Register(UserModel userModel) => await PostJson("register", userModel);

        public async Task<UserModel> DeleteUser(string username)
        {
            var httpResponseMessage = await Delete(username);
            if (httpResponseMessage == null)
                return null;
            if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                return new UserModel();

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<UserModel>(response);
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> ChangeUserName(string username, string newUsername) => 
            await PutForm($"user/{username}", new Dictionary<string, string> { { "newUsername", newUsername } });

        public async Task<HttpResponseMessage> Login(UserModel userModel) => await PostJson("login", userModel);
    }
}
