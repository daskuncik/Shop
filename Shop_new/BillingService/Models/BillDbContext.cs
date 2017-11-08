using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Models
{
    public class BillDbContext : DbContext
    {
        public DbSet<Billing> Billings { get; set; }

        public BillDbContext() : base() { }
        public BillDbContext(DbContextOptions<BillDbContext> ops) : base(ops) { }
    }
}
