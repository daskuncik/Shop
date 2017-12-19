using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shop_new.Models;

namespace AuthServer
{
    public class TokenDbContext : DbContext
    {
        public DbSet<Token> Tokens { get; set; }

        public TokenDbContext() : base() { }
        public TokenDbContext(DbContextOptions<TokenDbContext> ops) : base(ops) { }
    }
}
