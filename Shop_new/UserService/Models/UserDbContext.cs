using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
                Users.Add(new User { Name = "User1", Password = "pass1" });
                SaveChanges();
            }
        }
        public UserDbContext(DbContextOptions<UserDbContext> ops) : base(ops)
        {
            Initialize();
        }
    }
}
