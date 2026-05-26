using DataAggregator.Application.Abstractions;
using DataAggregator.Application.Configurations;
using DataAggregator.Application.Mappers;
using DataAggregator.Application.Models;
using DataAggregator.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataAggregator.Application
{
    public class DataProcessor
    {
        private readonly IEnformionClient _enformionClient;
        private readonly SnsPublisher _snsPublisher;
        private readonly IMongoDb _mongoDb;
        private readonly EnformionMapper _mapper;
        private readonly ILogger<DataProcessor> _logger;
        private readonly EnformionConfiguration _config;

        public DataProcessor(
            IEnformionClient enformionClient,
            SnsPublisher snsPublisher,
            IMongoDb mongoDb,
            EnformionMapper mapper,
            ILogger<DataProcessor> logger,
            IOptions<EnformionConfiguration> config)
        {
            _enformionClient = enformionClient;
            _snsPublisher = snsPublisher;
            _mongoDb = mongoDb;
            _mapper = mapper;
            _logger = logger;
            _config = config.Value;
        }

        public async Task ProcessAsync()
        {
            _logger.LogInformation("Starting data aggregation");

            await _mongoDb.EnsureIndexesAsync();

            var summary = await _enformionClient.GetSummaryAsync();
            var persistedSummary = _mapper.MapToPersistedSummary(summary, DateTime.UtcNow);
            await _mongoDb.SaveSummaryAsync(persistedSummary);

            _logger.LogInformation(
                "Summary persisted: {TotalRecords} total records across {ContentTypeCount} content types",
                summary.Totals.TotalRecords,
                summary.ContentTypes.Count);

            foreach (var contentType in summary.ContentTypes)
            {
                _logger.LogInformation("Fetching records for content type: {ContentType}", contentType.ContentType);

                var batch = new List<Record>();

                await foreach (var record in _enformionClient.GetRecordsAsync(
                    contentType.ContentType,
                    _config.RecordsPageSize))
                {
                    batch.Add(_mapper.MapToPersistedRecord(record, DateTime.UtcNow));

                    if (batch.Count >= _config.RecordsPageSize)
                    {
                        await _mongoDb.SaveRecordsAsync(batch);
                        _logger.LogDebug("Persisted batch of {Count} records for {ContentType}", batch.Count, contentType.ContentType);
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    await _mongoDb.SaveRecordsAsync(batch);
                    _logger.LogDebug("Persisted final batch of {Count} records for {ContentType}", batch.Count, contentType.ContentType);
                }

                _logger.LogInformation("Completed content type: {ContentType}", contentType.ContentType);
            }

            _logger.LogInformation("Data aggregation completed successfully");
        }
    }
}

