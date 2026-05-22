using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAggregator.Application.Models
{
    public class Summary
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("content_types")]
        public List<PersistedContentTypeSummary> ContentTypes { get; set; } = new();

        [BsonElement("totals")]
        public PersistedTotalSummary Totals { get; set; } = default!;

        [BsonElement("data_freshness")]
        public PersistedDataFreshness DataFreshness { get; set; } = default!;

        [BsonElement("fetched_at")]
        public DateTime FetchedAt { get; set; }
    }

    public class PersistedContentTypeSummary
    {
        [BsonElement("content_type")]
        public string ContentType { get; set; } = default!;

        [BsonElement("total_records")]
        public long TotalRecords { get; set; }

        [BsonElement("indexable_records")]
        public long IndexableRecords { get; set; }

        [BsonElement("estimated_sitemaps")]
        public int EstimatedSitemaps { get; set; }

        [BsonElement("latest_modified")]
        public DateTime LatestModified { get; set; }
    }

    public class PersistedTotalSummary
    {
        [BsonElement("total_records")]
        public long TotalRecords { get; set; }

        [BsonElement("indexable_records")]
        public long IndexableRecords { get; set; }

        [BsonElement("estimated_sitemaps")]
        public int EstimatedSitemaps { get; set; }
    }

    public class PersistedDataFreshness
    {
        [BsonElement("last_sync")]
        public DateTime LastSync { get; set; }

        [BsonElement("next_sync")]
        public DateTime NextSync { get; set; }
    }
}
