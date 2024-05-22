using Microsoft.EntityFrameworkCore;
using DemoLinqContainsOpenJson.DatabaseContext;
using DemoLinqContainsOpenJson.Models;
using System.Collections.Concurrent;
using System;
using Azure;
using System.Threading;

namespace DemoLinqContainsOpenJson.Classes
{
    public class QueryHelper : IQueryHelper
    {
        private readonly IDbContextFactory<LinqPerformanceDemoContext> _dbContextFactory;
        private readonly LinqPerformanceDemoContext _context;

        public QueryHelper(IDbContextFactory<LinqPerformanceDemoContext> dbContextFactory, LinqPerformanceDemoContext context)
        {
            _dbContextFactory = dbContextFactory;
            _context = context;
        }

        public async void StartProcessingAsync()
        {
            const int CHUNK_SIZE = 100_000;

            // ConcurrentBag to hold results
            var resultBag = new ConcurrentBag<List<LargeTable>>();

            // retrieve cached data (1M in each collection)
            List<int> cacheInt = await Retrieve1MCacheIntegerAsync();
            List<char> cacheChar = await Retrieve1MCacheCharAsync();
            List<double?> cacheDouble = await Retrieve1MCacheDoubleAsync();
            List<string> cacheString = await Retrieve1MCacheStringAsync();

            // chunk data into 100K batches for looping
            var chunkInt = cacheInt.Chunk(CHUNK_SIZE);
            var chunkChar = cacheChar.Chunk(CHUNK_SIZE);
            var chunkDouble = cacheDouble.Chunk(CHUNK_SIZE);
            var chunkString = cacheString.Chunk(CHUNK_SIZE);

            var tskDatabaseQueries = new List<Task>();

            // make a call using Integer
            Console.WriteLine($"Started Linq Contains - integer @{DateTime.Now}");
            tskDatabaseQueries.Add(Task.Run(async () =>
            {
                await Parallel.ForEachAsync(chunkInt, async (sublist, cancellationToken) =>
                {
                    var queryResult = await RetrieveIntegerChunk100KAsync(sublist);
                    resultBag.Add(queryResult);
                });
            }));
            Console.WriteLine($"Finished Linq Contains - integer @{DateTime.Now}");

            // make a call using Char
            Console.WriteLine($"Started Linq Contains - char @{DateTime.Now}");
            tskDatabaseQueries.Add(Task.Run(async () =>
            {
                await Parallel.ForEachAsync(chunkChar, async (sublist, cancellationToken) =>
                {
                    var queryResult = await RetrieveCharChunk100KAsync(sublist);
                    resultBag.Add(queryResult);
                });
            }));
            Console.WriteLine($"Finished Linq Contains - char @{DateTime.Now}");

            // make a call using string
            Console.WriteLine($"Started Linq Contains - string @{DateTime.Now}");
            tskDatabaseQueries.Add(Task.Run(async () =>
            {
                await Parallel.ForEachAsync(chunkString, async (sublist, cancellationToken) =>
                {
                    var queryResult = await RetrieveStringChunk100KAsync(sublist);
                    resultBag.Add(queryResult);
                });
            }));
            Console.WriteLine($"Finished Linq Contains - string @{DateTime.Now}");

            // make a call using Double
            Console.WriteLine($"Started Linq Contains - double @{DateTime.Now}");
            tskDatabaseQueries.Add(Task.Run(async () =>
            {
                await Parallel.ForEachAsync(chunkDouble, async (sublist, cancellationToken) =>
                {
                    var queryResult = await RetrieveDoubleChunk100KAsync(sublist);
                    resultBag.Add(queryResult);
                });
            }));
            Console.WriteLine($"Finished Linq Contains - double @{DateTime.Now}");

            // Wait for all tasks to complete
            await Task.WhenAll(tskDatabaseQueries);
           
            // display data
            DisplayQueryResults(resultBag.FirstOrDefault()?.Take(10)?.ToList());
        }

        #region Display Results
        /// <summary>
        /// Display a snapshot of data
        /// </summary>
        /// <param name="resultBag">A list containing a model</param>
        private void DisplayQueryResults(List<LargeTable> resultBag)
        {
            // Define the width for each column
            int columnPadding = 10;

            // Print header with fixed width columns
            Console.WriteLine($"{"Code Name".PadRight(columnPadding)} | {"Project Ref".PadRight(columnPadding)}");
            Console.WriteLine($"{new string('=', columnPadding)}   {new string('=', columnPadding)}");

            foreach (var record in resultBag)
            {
                Console.WriteLine($"{record.CodeName.PadRight(columnPadding)} | {record.ProjectRef.PadRight(columnPadding)}");
            }
        }
        #endregion

        #region Retrieve Cached Data
        private async Task<List<int>> Retrieve1MCacheIntegerAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Select(itm => itm.ID).Take(1_000_000).ToListAsync();
            }
        }

        private async Task<List<string>> Retrieve1MCacheStringAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Select(itm => itm.CodeName).Take(1_000_000).ToListAsync();
            }
        }

        private async Task<List<Double?>> Retrieve1MCacheDoubleAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Select(itm => itm.Height).Take(1_000_000).ToListAsync();
            }
        }

        private async Task<List<Char>> Retrieve1MCacheCharAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Select(itm => itm.SecurityType).Take(1_000_000).ToListAsync();
            }
        }

        #endregion

        #region Retrieve Queried Data - Chunking

        private async Task<List<LargeTable>> RetrieveIntegerChunk100KAsync(int[] chunkData)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Where(itm => chunkData.Contains(itm.ID)).ToListAsync();
            }
        }

        private async Task<List<LargeTable>> RetrieveStringChunk100KAsync(string[] chunkData)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Where(itm => chunkData.Contains(itm.ProjectRef)).ToListAsync();
            }
        }

        private async Task<List<LargeTable>> RetrieveDoubleChunk100KAsync(double?[] chunkData)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Where(itm => chunkData.Contains(itm.Height)).ToListAsync();
            }
        }

        private async Task<List<LargeTable>> RetrieveCharChunk100KAsync(char[] chunkData)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Where(itm => chunkData.Contains(itm.SecurityType)).ToListAsync();
            }
        }

        #endregion
    }
}