using DataAggregator.Application.Models;

namespace DataAggregator.Application.Mappers
{
    public class EnformionMapper
    {
        public Summary MapToPersistedSummary(EnformionSummaryResult source, DateTime fetchedAt)
        {
            return new Summary
            {
                ContentTypes = source.ContentTypes.Select(ct => new PersistedContentTypeSummary
                {
                    ContentType = ct.ContentType,
                    TotalRecords = ct.TotalRecords,
                    IndexableRecords = ct.IndexableRecords,
                    EstimatedSitemaps = ct.EstimatedSitemaps,
                    LatestModified = ct.LatestModified
                }).ToList(),
                Totals = new PersistedTotalSummary
                {
                    TotalRecords = source.Totals.TotalRecords,
                    IndexableRecords = source.Totals.IndexableRecords,
                    EstimatedSitemaps = source.Totals.EstimatedSitemaps
                },
                DataFreshness = new PersistedDataFreshness
                {
                    LastSync = source.DataFreshness.LastSync,
                    NextSync = source.DataFreshness.NextSync
                },
                FetchedAt = fetchedAt
            };
        }

        public Record MapToPersistedRecord(EnformionRecordResult source, DateTime persistedAt)
        {
            var data = new ContentTypeData
            {
                Attributes = source.Attributes ?? new Dictionary<string, string?>(),
                LastModified = source.LastModified,
                ChangeFrequency = source.ChangeFrequency,
                Priority = source.Priority,
                PersistedAt = persistedAt
            };

            return source.ContentType switch
            {
                "person" => new Record { TahoeId = source.Id, ContentType = "person", Person = data },
                "address" => new Record { TahoeId = source.Id, ContentType = "address", Address = data },
                "phone" => new Record { TahoeId = source.Id, ContentType = "phone", Phone = data },
                _ => throw new ArgumentOutOfRangeException(nameof(source.ContentType), $"Unknown content type: {source.ContentType}")
            };
        }
    }
}
