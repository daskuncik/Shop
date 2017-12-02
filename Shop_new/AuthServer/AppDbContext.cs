using AuthServer.Entities;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer
{
    public class AppDbContext : DbContext
    {
        public List<User> Users { get; set; } = new List<User>();

        public AppDbContext() : base()
        {
            Initialize();
        }

        public AppDbContext(DbContextOptions<AppDbContext> ops) : base(ops)
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!Users.Any())
            {
                Users.Add(new User { Id = 1, Username = "User1", Password = "pass1".Sha256() });
                Users.Add(new User { Id = 2, Username = "User2", Password = "pass2".Sha256() });
                Users.Add(new User { Id = 3, Username = "User3", Password = "pass3".Sha256() });
            }
        }
    }
}
