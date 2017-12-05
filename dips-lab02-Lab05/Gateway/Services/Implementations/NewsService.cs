using Gateway.Models;
using Gateway.Pagination;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Services.Implementations
{
    public class NewsService : Service, INewsService
    {
        public NewsService(IConfiguration configuration) 
            : base(configuration.GetSection("Addresses")["News"]) { }

        public async Task<HttpResponseMessage> AddNews(NewsModel newsModel) => await PostJson("", newsModel);

        public async Task<PaginatedList<string>> GetNewsByUser(string username, int page, int perpage)
        {
            var httpResponseMessage = await Get($"author/{username}?page={page}&perpage={perpage}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<PaginatedList<string>>(response);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<NewsModel>> DeleteNewsWithAuthor(string username)
        {
            var httpResponseMessage = await Delete($"author/{username}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;

            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<List<NewsModel>>(response);
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> ChangeUserName(string username, string newUsername) => 
            await PutForm($"user/{username}", new Dictionary<string, string> { { "newUsername", newUsername } });
    }
}
