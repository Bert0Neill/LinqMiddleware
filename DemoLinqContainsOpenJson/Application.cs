using Microsoft.EntityFrameworkCore;
using DemoLinqContainsOpenJson.DatabaseContext;

public class Application
{
    private readonly LinqPerformanceDemoContext _context;

    public Application(LinqPerformanceDemoContext context)
    {
        _context = context;
    }

    public async Task RunAsync()
    {
        var records = await _context.LargeTables.Take(100).ToListAsync();
        foreach (var record in records)
        {
            Console.WriteLine($"{record.CodeName} {record.ProjectRef}");
        }
    }
}

