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
    public class SubscriptionsService : Service, ISubscriptionsService
    {
        public SubscriptionsService(IConfiguration configuration)
            : base(configuration.GetSection("Addresses")["Subscriptions"]) { }

        public async Task<PaginatedList<string>> GetSubscribedAuthorsForName(string name, int page, int perpage)
        {
            var httpResponseMessage = await Get($"{name}?page={page}&perpage={perpage}");
            if (httpResponseMessage == null || httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Forbidden || httpResponseMessage.Content == null)
                return null;
            try
            {
                return JsonConvert.DeserializeObject<PaginatedList<string>>(await httpResponseMessage.Content.ReadAsStringAsync());
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> AddSubscription(string subscriber, string author)
        {
            return await PostForm($"{subscriber}", new Dictionary<string, string> { { "author", author } });
        }

        public async Task<HttpResponseMessage> RemoveSubscription(string subscriber, string author)
        {
            return await Delete($"{subscriber}/{author}");
        }

        public async Task<List<string>> GetAllAssociatedSubscriptions(string username, int page, int perpage)
        {
            var httpResponseMessage = await Get($"all/{username}?page={page}&perpage={perpage}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;
            try
            {
                return JsonConvert.DeserializeObject<List<string>>(await httpResponseMessage.Content.ReadAsStringAsync());
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Tuple<string, string>>> RemoveAllAssociatedSubscriptions(string username)
        {
            var httpResponseMessage = await Delete($"all/{username}");
            if (httpResponseMessage == null || httpResponseMessage.Content == null)
                return null;
            try
            {
                return JsonConvert.DeserializeObject<List<Tuple<string, string>>>(await httpResponseMessage.Content.ReadAsStringAsync());
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> ChangeUserName(string username, string newUsername)
        {
            HttpResponseMessage httpResponseMessage = await PutForm($"user/{username}", new Dictionary<string, string> { { "newUsername", newUsername } });
            if (httpResponseMessage?.StatusCode == System.Net.HttpStatusCode.Forbidden)
                httpResponseMessage = null;
            return httpResponseMessage;
        }
    }
}
