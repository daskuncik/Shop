using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionManager
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Subscription> Subscriptions { get; set; }
        public ApplicationDbContext() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!Subscriptions.Any())
            {
                Subscriptions.Add(new Subscription { Author = "User2", Subscriber = "User1" });
                Subscriptions.Add(new Subscription { Author = "User3", Subscriber = "User1" });
                Subscriptions.Add(new Subscription { Author = "User3", Subscriber = "User2" });
                SaveChanges();
            }
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> ops) : base(ops)
        {
            Initialize();
        }
    }
}
