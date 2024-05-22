using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace DemoLinqContainsOpenJson.DatabaseContext
{
    public class DbContextFactory<TContext> : IDbContextFactory<TContext> where TContext : DbContext, new()
    {
        public TContext CreateDbContext()
        {
            return new TContext();
        }
    }
}
