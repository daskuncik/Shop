using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shop_new;

namespace UserService.Models
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

       

        public UserDbContext() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!Users.Any())
            {
                Users.Add(new User { Name = "User1", Password = "pass1".Sha256() });
                Users.Add(new User { Name = "User2", Password = "pass2".Sha256() });
                SaveChanges();
            }
        }
        public UserDbContext(DbContextOptions<UserDbContext> ops) : base(ops)
        {
            Initialize();
        }
    }
}
