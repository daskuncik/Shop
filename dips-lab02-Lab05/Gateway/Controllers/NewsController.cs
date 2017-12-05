using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Gateway.Models.News;
using Gateway.Pagination;
using Gateway.Models.Shared;
using Gateway.CustomAuthorization;

namespace Gateway.Controllers
{
    [Route("news")]
    public class NewsController : Controller
    {
        private GatewayController gatewayController;

        public NewsController(GatewayController gatewayController)
        {
            this.gatewayController = gatewayController;
        }

        [HttpGet]
        public async Task<IActionResult> Index(IndexModel indexModel)
        {
            if (Request.Headers.Keys.Contains(CustomAuthorizationMiddleware.UserWord))
                indexModel.Username = string.Join(string.Empty, Request.Headers[CustomAuthorizationMiddleware.UserWord]);
            if (!string.IsNullOrWhiteSpace(indexModel.Username))
            {
                var news = await gatewayController.GetNews(indexModel.Username, indexModel.Page, indexModel.Size);
                if (news.StatusCode == 200)
                {
                    if (news.Value as string != null && string.IsNullOrWhiteSpace(news.Value as string))
                        return View(indexModel);

                    PaginatedList<string> paginatedList = (news.Value as PaginatedList<string>);
                    indexModel.News = paginatedList.Content.Select(s =>
                    {
                        var arr = s.Split(Environment.NewLine);
                        var header = arr[0];
                        header = header.Substring(header.IndexOf(' '));
                        var body = arr[1];
                        body = body.Substring(body.IndexOf(' '));
                        var author = arr[2];
                        author = author.Substring(author.IndexOf(' '));
                        return new Tuple<string, string, string>(header, body, author);
                    }).ToList();
                    indexModel.Page = paginatedList.Page;
                    indexModel.MaxPage = paginatedList.MaxPage;
                    indexModel.Size = paginatedList.Size;
                    return View(indexModel);
                }
                else
                    return View("Error", new ErrorModel(news));
            }
            else
                return RedirectToAction(nameof(Authenticate));
        }

        [HttpGet("authenticate")]
        public async Task<IActionResult> Authenticate()
        {
            return View();
        }

        [HttpGet("add")]
        public async Task<IActionResult> Add(string username)
        {
            return View(new AddModel { Username = username });
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(AddModel addModel)
        {
            var result = await gatewayController.AddNews(new Models.NewsModel { Author = addModel.Username, Body = addModel.Body, Header = addModel.Header });
            if (result.StatusCode == 200)
                return RedirectToAction(nameof(Index), new IndexModel { Username = addModel.Username });
            return View("Error", new ErrorModel(result));
        }
    }
}