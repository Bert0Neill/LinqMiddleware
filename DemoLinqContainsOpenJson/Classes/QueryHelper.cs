using Microsoft.EntityFrameworkCore;
using DemoLinqContainsOpenJson.DatabaseContext;
using DemoLinqContainsOpenJson.Models;

namespace DemoLinqContainsOpenJson.Classes
{
    public class QueryHelper : IQueryHelper
    {
        private readonly LinqPerformanceDemoContext _context;

        public QueryHelper(LinqPerformanceDemoContext context)
        {
            _context = context;
        }

        public async Task DisplayTop10Async()
        {
            // Define the width for each column
            int columnPadding = 10;            

            // make DB query
            var records = await _context.LargeTables.OrderBy(itm => itm.ID).Take(10).ToListAsync();

            // Print header with fixed width columns
            Console.WriteLine($"{"Code Name".PadRight(columnPadding)} | {"Project Ref".PadRight(columnPadding)}");
            Console.WriteLine($"{new string('=', columnPadding)}   {new string('=', columnPadding)}");

            // Print records with fixed width columns
            foreach (var record in records)
            {
                Console.WriteLine($"{record.CodeName.PadRight(columnPadding)} | {record.ProjectRef.PadRight(columnPadding)}");
            }
        }

        public async Task<List<LargeTable>> RetrieveTop100Async()
        {
            return await _context.LargeTables.OrderBy(itm => itm.ID).Take(100).ToListAsync();
        }

        public async Task<List<int>> Retrieve1MillonIdsAsync()
        {
            return await _context.LargeTables.Select(itm => itm.ID).Take(1000000).ToListAsync();
        }
        public async Task<List<LargeTable>> RetrieveIdChunk100KAsync(List<int> chunkData)
        {
            return await _context.LargeTables.Where(itm => chunkData.Contains(itm.ID)).ToListAsync();
        }

        public async Task<List<LargeTable>> RetrieveStringChunk100KAsync(List<string> chunkData)
        {
            return await _context.LargeTables.Where(itm => chunkData.Contains(itm.ProjectRef)).ToListAsync();
        }

        public async Task<List<LargeTable>> RetrieveDoubleChunk100KAsync(List<double?> chunkData)
        {
            return await _context.LargeTables.Where(itm => chunkData.Contains(itm.Height)).ToListAsync();
        }

        public async Task<List<LargeTable>> RetrieveCharChunk100KAsync(List<char> chunkData)
        {
            return await _context.LargeTables.Where(itm => chunkData.Contains(itm.SecurityType)).ToListAsync();
        }

        public async Task<List<LargeTable>> RetrieveBooleanChunk100KAsync(List<bool?> chunkData)
        {
            return await _context.LargeTables.Where(itm => chunkData.Contains(itm.IsActive)).ToListAsync();
        }
    }
}