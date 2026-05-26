using DataAggregator.Application.Models;

namespace DataAggregator.Application.Abstractions
{
    public interface IEnformionClient
    {
        Task<EnformionSummaryResult> GetSummaryAsync();
        IAsyncEnumerable<EnformionRecordResult> GetRecordsAsync(string contentType, int limit, string? cursor = null);
    }
}
