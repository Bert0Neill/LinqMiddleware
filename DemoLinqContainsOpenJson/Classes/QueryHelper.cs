using DemoLinqContainsOpenJson.DatabaseContext;
using DemoLinqContainsOpenJson.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace DemoLinqContainsOpenJson.Classes
{
    public class QueryHelper : IQueryHelper
    {
        private readonly IDbContextFactory<LinqPerformanceDemoContext> _dbContextFactory;
        private List<string> _listNotIn = new List<string>() { "Finn", "Harps", "FC" };

        public QueryHelper(IDbContextFactory<LinqPerformanceDemoContext> dbContextFactory, LinqPerformanceDemoContext context)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async void StartProcessingAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine($"Start Time: {DateTime.Now}");

            const int CHUNK_SIZE = 40_000;            
            var tskDatabaseQueries = new List<Task>(); // maintain the running background tasks
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 2 };
            var resultBag = new ConcurrentBag<List<LargeTable>>(); // ConcurrentBag to hold all results safely

            // mock a client that uses a cache of data to filter against - retrieve cached data (1M in each collection) - data will be duplicated in cases like char!
            List<int> cacheInt = await Retrieve1MCacheIntegerAsync();
            List<char> cacheChar = await Retrieve1MCacheCharAsync();
            List<double?> cacheDouble = await Retrieve1MCacheDoubleAsync();
            List<string> cacheString = await Retrieve1MCacheStringAsync();

            //// test strings with single quote!
            //int middleIndex = cacheString[1].Length / 2;
            //cacheString[1] = cacheString[1].Insert(middleIndex, "'");
            //cacheString[0] = cacheString[0].Insert(middleIndex, "'");


            // chunk data into 100K batches for looping
            var chunkInt = cacheInt.Chunk(CHUNK_SIZE);
            var chunkChar = cacheChar.Chunk(CHUNK_SIZE);
            var chunkDouble = cacheDouble.Chunk(CHUNK_SIZE);
            var chunkString = cacheString.Chunk(CHUNK_SIZE);

            // make a linq call using Integer as the IN clause
            tskDatabaseQueries.Add(Task.Run(async () =>
            {
                await Parallel.ForEachAsync(chunkInt, parallelOptions, async (sublist, cancellationToken) =>
                {
                    var queryResult = await RetrieveIntegerChunkAsync(sublist);
                    resultBag.Add(queryResult);
                });
            }));

            // make a linq call using Char as the IN clause
            tskDatabaseQueries.Add(Task.Run(async () =>
            {
                await Parallel.ForEachAsync(chunkChar, parallelOptions, async (sublist, cancellationToken) =>
                {
                    var queryResult = await RetrieveCharChunkAsync(sublist);
                    resultBag.Add(queryResult);
                });
            }));

            // make a linq call using Double as the IN clause
            tskDatabaseQueries.Add(Task.Run(async () =>
            {
                await Parallel.ForEachAsync(chunkDouble, parallelOptions, async (sublist, cancellationToken) =>
                {
                    var queryResult = await RetrieveDoubleChunkAsync(sublist);
                    resultBag.Add(queryResult);
                });
            }));

            // make a linq call using String as the IN clause
            tskDatabaseQueries.Add(Task.Run(async () =>
            {
                await Parallel.ForEachAsync(chunkString, parallelOptions, async (sublist, cancellationToken) =>
                {
                    var queryResult = await RetrieveStringChunkAsync(sublist);
                    resultBag.Add(queryResult);
                });
            }));

            // Wait for all tasks to complete
            await Task.WhenAll(tskDatabaseQueries);

            Console.WriteLine($"End Time: {DateTime.Now}");

            // display a snapshot of data to confirm queries worked
            DisplayQueryResults(resultBag?.FirstOrDefault()?.ToList());

            stopwatch.Stop();

            // Get the elapsed time
            double elapsedTimeInSeconds = stopwatch.Elapsed.TotalSeconds;

            // Display the elapsed time
            Console.WriteLine($"Elapsed Time: {elapsedTimeInSeconds}");

        }

        #region Display Results
        /// <summary>
        /// Display a snapshot of data
        /// </summary>
        /// <param name="resultBag">A list containing a model</param>
        private void DisplayQueryResults(List<LargeTable> resultBag)
        {            
            const int columnPadding = 10; // Define the width for each column
            const int dataSize = 10; // the number of record to display in console

            Console.WriteLine($"Result Count: {resultBag.Count}"); 

            // Print header with fixed width columns
            Console.WriteLine($"{"Code Name".PadRight(columnPadding)} | {"Project Ref".PadRight(columnPadding)}");
            Console.WriteLine($"{new string('=', columnPadding)}   {new string('=', columnPadding)}");

            foreach (var record in resultBag.Take(dataSize))
            {
                Console.WriteLine($"{record.CodeName.PadRight(columnPadding)} | {record.ProjectRef.PadRight(columnPadding)}");
            }
        }
        #endregion

        #region Retrieve Cached Data
        private async Task<List<int>> Retrieve1MCacheIntegerAsync(int recoundCount = 1_000_000)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Select(itm => itm.ID).Take(recoundCount).ToListAsync();
            }
        }

        private async Task<List<string>> Retrieve1MCacheStringAsync(int recoundCount = 1_000_000)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Select(itm => itm.CodeName).Take(recoundCount).ToListAsync();
            }
        }

        private async Task<List<Double?>> Retrieve1MCacheDoubleAsync(int recoundCount = 1_000_000)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Select(itm => itm.Height).Take(recoundCount).ToListAsync();
            }
        }

        private async Task<List<Char>> Retrieve1MCacheCharAsync(int recoundCount = 1_000_000)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Select(itm => itm.SecurityType).Take(recoundCount).ToListAsync();
            }
        }

        #endregion

        #region Retrieve Queried Data - Chunking (LINQ query includes IN and NOT IN clauses)

        private async Task<List<LargeTable>> RetrieveIntegerChunkAsync(int[] chunkData)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Where(itm => chunkData.Contains(itm.ID) && !_listNotIn.Contains(itm.ProjectRef)).ToListAsync();
            }
        }

        private async Task<List<LargeTable>> RetrieveStringChunkAsync(string[] chunkData)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Where(itm => chunkData.Contains(itm.CodeName) && !_listNotIn.Contains(itm.ProjectRef)).ToListAsync();
            }
        }

        private async Task<List<LargeTable>> RetrieveDoubleChunkAsync(double?[] chunkData)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Where(itm => chunkData.Contains(itm.Height) && !_listNotIn.Contains(itm.ProjectRef)).ToListAsync();
            }
        }

        private async Task<List<LargeTable>> RetrieveCharChunkAsync(char[] chunkData)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.LargeTables.Where(itm => chunkData.Contains(itm.SecurityType) && !_listNotIn.Contains(itm.ProjectRef)).ToListAsync();
            }
        }

        #endregion
    }
}