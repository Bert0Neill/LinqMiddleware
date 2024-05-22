using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using DemoLinqContainsOpenJson.DatabaseContext;
using System;
using DemoLinqContainsOpenJson.Classes;

var builder = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
{
    services.AddDbContext<LinqPerformanceDemoContext>();
    services.AddTransient<IQueryHelper, QueryHelper>(); // DI helper class
}).Build();

var app = builder.Services.GetRequiredService<IQueryHelper>();
await app.DisplayTop10Async();

Console.WriteLine("\r\nPress any key to continue ...");
Console.Read();
