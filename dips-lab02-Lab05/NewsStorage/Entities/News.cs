using Gateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsStorage.Entities
{
    public class News
    {
        public News() { }

        public News(NewsModel newsModel)
        {
            this.Author = newsModel.Author;
            this.Body = newsModel.Body;
            this.Header = newsModel.Header;
        }

        public int Id { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
    }
}
