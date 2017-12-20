using Microsoft.EntityFrameworkCore;
using Statistics.Entities;

namespace Statistics
{
    public class AppDbContext : DbContext
    {
        public DbSet<RequestInfo> Requests { get; set; }
        public DbSet<LoginInfo> Logins { get; set; }
        public DbSet<OrderOperationInfo> OrderOperations { get; set; }
        public DbSet<GoodsOperationInfo> GoodOperations { get; set; }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected AppDbContext()
        {
        }
    }
}
