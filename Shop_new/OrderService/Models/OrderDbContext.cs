using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Models
{
    public class OrderDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderUnit> OrderUnits { get; set; }

        public OrderDbContext() : base() { }
        public OrderDbContext(DbContextOptions<OrderDbContext> ops) : base(ops) { }

    }
}