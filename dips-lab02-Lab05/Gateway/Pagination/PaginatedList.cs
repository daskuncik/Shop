using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Pagination
{
    public class PaginatedList<T> 
    {
        public PaginatedList(List<T> content, int size, int page, int maxPage)
        {
            Size = size;
            Content = content;
            Page = page;
            MaxPage = maxPage;
        }

        public int Size { get; private set; }
        public List<T> Content { get; private set; }
        public int Page { get; private set; }
        public int MaxPage { get; private set; }
    }
}
