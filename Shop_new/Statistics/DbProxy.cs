using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics
{
    public class DbProxy
    {
        public DbProxy(AppDbContext dbContext)
        {
            DbContext = dbContext;
        }
        public AppDbContext DbContext { get; private set; }
    }
}
