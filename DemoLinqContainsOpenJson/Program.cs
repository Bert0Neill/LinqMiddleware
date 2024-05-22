using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using DemoLinqContainsOpenJson.DatabaseContext;
using System;
using DemoLinqContainsOpenJson.Classes;
using System.Collections.Concurrent;
using DemoLinqContainsOpenJson.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
{
    services.AddDbContextFactory<LinqPerformanceDemoContext>(options =>
    {
        // Configure your DbContext options here if needed
        options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection"));
    });

    services.AddDbContext<LinqPerformanceDemoContext>();
    services.AddTransient<IQueryHelper, QueryHelper>(); // DI helper class

}).Build();

var queryHelper = builder.Services.GetRequiredService<IQueryHelper>(); // create an instance of QueryHelper class
queryHelper.StartProcessingAsync();

Console.WriteLine("\r\nPress any key to continue ...");
Console.Read();
