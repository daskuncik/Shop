using Gateway.Models;
using Gateway.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public interface INewsService
    {
        Task<HttpResponseMessage> AddNews(NewsModel newsModel);
        Task<PaginatedList<string>> GetNewsByUser(string username, int page, int perpage);
        Task<List<NewsModel>> DeleteNewsWithAuthor(string username);
        Task<HttpResponseMessage> ChangeUserName(string username, string newUsername);
    }
}
