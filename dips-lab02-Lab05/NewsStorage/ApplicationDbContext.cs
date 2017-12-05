using Microsoft.EntityFrameworkCore;
using NewsStorage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsStorage
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<News> News { get; set; }
        public ApplicationDbContext() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!News.Any())
            {
                News.Add(new News { Author = "User2", Header = "Hot news from user 2!", Body = "Body of hot news", Date = DateTime.Now });
                News.Add(new News { Author = "User3", Header = "Hot news from user 3!", Body = "Body of hot news", Date = DateTime.Now });
                News.Add(new News { Author = "User3", Header = "Hot news from user 3! Second edition", Body = "Body of hot news", Date = DateTime.Now });
                News.Add(new News { Author = "User1", Header = "Hot news from user 1!", Body = "Body of hot news", Date = DateTime.Now });
                News.Add(new News { Author = "User2", Header = "Hot news from user 2!", Body = "Body of hot news", Date = DateTime.Now });
                SaveChanges();
            }
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> ops) : base(ops)
        {
            Initialize();
        }
    }
}
