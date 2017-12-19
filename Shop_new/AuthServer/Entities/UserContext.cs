using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Entities
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options)
           : base(options)
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!Users.Any())
            {
                Users.Add(new User { Username = "User1", Password = "pass1".Sha256() });
                Users.Add(new User { Username = "User2", Password = "pass2".Sha256() });
                Users.Add(new User { Username = "User3", Password = "pass3".Sha256() });
                SaveChanges();
            }
        }

        public UserContext()
            : base()
        {
            Initialize();
        }
        public DbSet<User> Users { get; set; }
    }
}
