using DataAggregator.Application.Models;

namespace DataAggregator.Application.Abstractions
{
    public interface IMongoDb
    {
        Task EnsureIndexesAsync();
        Task SaveSummaryAsync(Summary summary);
        Task SaveRecordsAsync(IReadOnlyList<Record> records);
    }
}
