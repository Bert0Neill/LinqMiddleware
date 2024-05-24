using DemoLinqContainsOpenJson.Classes;
using DemoLinqContainsOpenJson.DatabaseContext;
using LinqOpenJsonInClauseMiddleware.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
{
    services.AddTransient<IQueryHelper, QueryHelper>(); // DI helper class

    services.AddDbContextFactory<LinqPerformanceDemoContext>(options =>
    {
        options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection"));

        options.AddInterceptors(new SqlServerReaderExecutingOpenJson()); // add to dbContext here or in dbContext file

    }, ServiceLifetime.Scoped);

    services.AddDbContext<LinqPerformanceDemoContext>(options =>
    {
        options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection"));

        options.AddInterceptors(new SqlServerReaderExecutingOpenJson()); // add to dbContext here or in dbContext file

    }, ServiceLifetime.Scoped);

}).Build();

var queryHelper = builder.Services.GetRequiredService<IQueryHelper>(); // create an instance of QueryHelper class
queryHelper.StartProcessingAsync(); // start processing

Console.WriteLine("\r\nPress any key to continue ...");
Console.Read();
