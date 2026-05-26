using System.Text.Json.Serialization;

namespace DataAggregator.Application.Models
{
    public class EnformionSummaryResult
    {
        [JsonPropertyName("content_types")]
        public List<ContentTypeSummary> ContentTypes { get; set; } = default!;
        [JsonPropertyName("totals")]
        public TotalSummary Totals { get; set; } = default!;
        [JsonPropertyName("data_freshness")]
        public DataFreshness DataFreshness { get; set; } = default!;
    }

    public class ContentTypeSummary
    {
        [JsonPropertyName("content_type")]
        public string ContentType { get; set; } = default!;
        [JsonPropertyName("total_records")]
        public long TotalRecords { get; set; }
        [JsonPropertyName("indexable_records")]
        public long IndexableRecords { get; set; }
        [JsonPropertyName("estimated_sitemaps")]
        public int EstimatedSitemaps { get; set; }
        [JsonPropertyName("latest_modified")]
        public DateTime LatestModified { get; set; }
    }

    public class TotalSummary
    {
        [JsonPropertyName("total_records")]
        public long TotalRecords { get; set; }
        [JsonPropertyName("indexable_records")]
        public long IndexableRecords { get; set; }
        [JsonPropertyName("estimated_sitemaps")]
        public int EstimatedSitemaps { get; set; }
    }

    public class DataFreshness
    {
        [JsonPropertyName("last_sync")]
        public DateTime LastSync { get; set; }
        [JsonPropertyName("next_sync")]
        public DateTime NextSync { get; set; }
    }
}
