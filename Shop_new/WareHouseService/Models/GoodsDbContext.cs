using System;
using System.Collections.Generic;
using System.Linq;
using WareHouseService.Models;
using Microsoft.EntityFrameworkCore;

namespace WareHouseService.Models
{
    public class GoodsDbContext : DbContext
    {
        public DbSet<Good> Goods { get; set; }

        public GoodsDbContext() : base() { }
        public GoodsDbContext(DbContextOptions<GoodsDbContext> ops) : base(ops) { }
    }
}