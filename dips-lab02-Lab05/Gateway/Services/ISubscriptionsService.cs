using Gateway.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public interface ISubscriptionsService
    {
        Task<PaginatedList<string>> GetSubscribedAuthorsForName(string name, int page, int perpage);
        Task<HttpResponseMessage> AddSubscription(string subscriber, string author);
        Task<HttpResponseMessage> RemoveSubscription(string subscriber, string author);
        Task<List<string>> GetAllAssociatedSubscriptions(string username, int page, int perpage);
        Task<List<Tuple<string, string>>> RemoveAllAssociatedSubscriptions(string username);
        Task<HttpResponseMessage> ChangeUserName(string username, string newUsername);
    }
}
