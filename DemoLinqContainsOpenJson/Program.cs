using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using DemoLinqContainsOpenJson.DatabaseContext;

var builder = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
{
    services.AddDbContext<LinqPerformanceDemoContext>();
    services.AddTransient<Application>();
}).Build();

var app = builder.Services.GetRequiredService<Application>();
await app.RunAsync();

Console.WriteLine("\r\nPress any key to continue ...");
Console.Read();

