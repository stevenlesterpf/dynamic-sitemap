using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAggregator.Application.Models
{
    public class Record
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("tahoe_id")]
        public string TahoeId { get; set; } = default!;

        [BsonElement("person")]
        public ContentTypeData? Person { get; set; }

        [BsonElement("address")]
        public ContentTypeData? Address { get; set; }

        [BsonElement("phone")]
        public ContentTypeData? Phone { get; set; }

        /// <summary>Transient — not persisted. Used to route the upsert to the correct field.</summary>
        [BsonIgnore]
        public string ContentType { get; set; } = default!;
    }

    public class ContentTypeData
    {
        [BsonElement("attrs")]
        public Dictionary<string, string?> Attributes { get; set; } = new();

        [BsonElement("last_modified")]
        public DateTime LastModified { get; set; }

        [BsonElement("change_frequency")]
        public string ChangeFrequency { get; set; } = default!;

        [BsonElement("priority")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Priority { get; set; }

        [BsonElement("persisted_at")]
        public DateTime PersistedAt { get; set; }
    }
}
