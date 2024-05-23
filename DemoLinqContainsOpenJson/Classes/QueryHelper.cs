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
        private List<string> _listNotIn = new List<string>() { "Finn", "Harps", "FC" };

        public QueryHelper(IDbContextFactory<LinqPerformanceDemoContext> dbContextFactory, LinqPerformanceDemoContext context)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async void StartProcessingAsync()
        {
            const int CHUNK_SIZE = 100_000;
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 2 };

            // ConcurrentBag to hold all results
            var resultBag = new ConcurrentBag<List<LargeTable>>();

            // retrieve cached data (1M in each collection)
            List<int> cacheInt = await Retrieve1MCacheIntegerAsync();
            List<char> cacheChar = await Retrieve1MCacheCharAsync();
            List<double?> cacheDouble = await Retrieve1MCacheDoubleAsync();
            List<string> cacheString = await Retrieve1MCacheStringAsync();

            //// chunk data into 100K batches for looping
            var chunkInt = cacheInt.Chunk(CHUNK_SIZE);
            var chunkChar = cacheChar.Chunk(CHUNK_SIZE);
            var chunkDouble = cacheDouble.Chunk(CHUNK_SIZE);
            var chunkString = cacheString.Chunk(CHUNK_SIZE);

            var tskDatabaseQueries = new List<Task>();

            //// make a call using Integer
            //tskDatabaseQueries.Add(Task.Run(async () =>
            //{
            //    await Parallel.ForEachAsync(chunkInt, parallelOptions, async (sublist, cancellationToken) =>
            //    {
            //        var queryResult = await RetrieveIntegerChunkAsync(sublist);
            //        resultBag.Add(queryResult);
            //    });
            //}));

            //make a call using Char
            tskDatabaseQueries.Add(Task.Run(async () =>
            {
                await Parallel.ForEachAsync(chunkChar, parallelOptions, async (sublist, cancellationToken) =>
                {
                    var queryResult = await RetrieveCharChunkAsync(sublist);
                    resultBag.Add(queryResult);
                });
            }));

            //// make a call using string
            //tskDatabaseQueries.Add(Task.Run(async () =>
            //{
            //    await Parallel.ForEachAsync(chunkString, parallelOptions, async (sublist, cancellationToken) =>
            //    {
            //        var queryResult = await RetrieveStringChunkAsync(sublist);
            //        resultBag.Add(queryResult);
            //    });
            //}));

            //// make a call using Double
            //tskDatabaseQueries.Add(Task.Run(async () =>
            //{
            //    await Parallel.ForEachAsync(chunkDouble, parallelOptions, async (sublist, cancellationToken) =>
            //    {
            //        var queryResult = await RetrieveDoubleChunkAsync(sublist);
            //        resultBag.Add(queryResult);
            //    });
            //}));

            // Wait for all tasks to complete
            await Task.WhenAll(tskDatabaseQueries);

            // display data
            DisplayQueryResults(resultBag?.FirstOrDefault()?.ToList());
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

            Console.WriteLine($"Result Count: {resultBag.Count}"); 

            // Print header with fixed width columns
            Console.WriteLine($"{"Code Name".PadRight(columnPadding)} | {"Project Ref".PadRight(columnPadding)}");
            Console.WriteLine($"{new string('=', columnPadding)}   {new string('=', columnPadding)}");

            foreach (var record in resultBag.Take(10))
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