using System.Text.Json.Serialization;

namespace DataAggregator.Application.Models
{
    public class EnformionRecordsResult
    {
        [JsonPropertyName("data")]
        public List<EnformionRecordResult> Data { get; set; } = default!;
        [JsonPropertyName("pagination")]
        public EnformionRecordsPagination Pagination { get; set; } = default!;

    }

    public class EnformionRecordResult
    {
        [JsonPropertyName("tahoe_id")]
        public string Id { get; set; } = default!;
        [JsonPropertyName("content_type")]
        public string ContentType { get; set; } = default!;
        [JsonPropertyName("attrs")]
        public Dictionary<string, string?> Attributes { get; set; } = default!;
        [JsonPropertyName("last_modified")]
        public DateTime LastModified { get; set; }
        [JsonPropertyName("change_frequency")]
        public string ChangeFrequency { get; set; } = default!;
        [JsonPropertyName("priority")]
        public decimal Priority { get; set; } = default!;
    }

    public class EnformionRecordsPagination
    {
        [JsonPropertyName("total")]
        public long Total { get; set; }
        [JsonPropertyName("limit")]
        public int limit { get; set; }
        [JsonPropertyName("offset")]
        public int offset { get; set; }
        [JsonPropertyName("has_more")]
        public bool HasMore { get; set; }
        [JsonPropertyName("next_cursor")]
        public string NextCursor { get; set; } = default!;
    }
}
