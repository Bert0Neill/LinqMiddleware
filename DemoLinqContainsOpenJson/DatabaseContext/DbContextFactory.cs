using Microsoft.EntityFrameworkCore;

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
