using Gateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public interface IAccountsService
    {
        Task<HttpResponseMessage> CheckIfUserExists(UserModel existsModel);
        Task<HttpResponseMessage> Login(UserModel userModel);
        Task<HttpResponseMessage> Register(UserModel userModel);
        Task<UserModel> DeleteUser(string username);
        Task<HttpResponseMessage> ChangeUserName(string username, string newUsername);
    }
}
