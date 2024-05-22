using DemoLinqContainsOpenJson.Models;

public interface IQueryHelper
{
    Task DisplayTop10Async();
    Task<List<int>> Retrieve1MillonIdsAsync();
    Task<List<LargeTable>> RetrieveBooleanChunk100KAsync(List<bool?> chunkData);
    Task<List<LargeTable>> RetrieveCharChunk100KAsync(List<char> chunkData);
    Task<List<LargeTable>> RetrieveDoubleChunk100KAsync(List<double?> chunkData);
    Task<List<LargeTable>> RetrieveIdChunk100KAsync(List<int> chunkData);
    Task<List<LargeTable>> RetrieveStringChunk100KAsync(List<string> chunkData);
    Task<List<LargeTable>> RetrieveTop100Async();
}