using DataAggregator.Application.Abstractions;
using DataAggregator.Application.Configurations;
using DataAggregator.Application.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DataAggregator.Infrastructure.Persistence
{
    public class MongoDb : IMongoDb
    {
        private readonly IMongoCollection<Summary> _summaryCollection;
        private readonly IMongoCollection<Record> _recordsCollection;

        public MongoDb(IMongoClient mongoClient, IOptions<MongoDbConfiguration> config)
        {
            var database = mongoClient.GetDatabase(config.Value.DatabaseName);
            _summaryCollection = database.GetCollection<Summary>(config.Value.SummaryCollectionName);
            _recordsCollection = database.GetCollection<Record>(config.Value.RecordsCollectionName);
        }

        public async Task EnsureIndexesAsync()
        {
            await _recordsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Record>(
                    Builders<Record>.IndexKeys.Ascending(x => x.TahoeId),
                    new CreateIndexOptions { Unique = true, Name = "idx_tahoe_id_unique" }));
        }

        public async Task SaveSummaryAsync(Summary summary)
        {
            await _summaryCollection.InsertOneAsync(summary, options: null);
        }

        public async Task SaveRecordsAsync(IReadOnlyList<Record> records)
        {
            if (records.Count == 0)
                return;

            var writes = records.Select((Func<Record, UpdateOneModel<Record>>)(record =>
            {
                var filter = Builders<Record>.Filter.Eq(x => x.TahoeId, record.TahoeId);

                var update = record.ContentType switch
                {
                    "person"  => Builders<Record>.Update.Set(x => x.Person,  record.Person),
                    "address" => Builders<Record>.Update.Set(x => x.Address, record.Address),
                    "phone"   => Builders<Record>.Update.Set(x => x.Phone,   record.Phone),
                    _ => throw new InvalidOperationException($"Unknown content type: {record.ContentType}")
                };

                return new UpdateOneModel<Record>(
                    filter,
                    UpdateDefinitionExtensions.SetOnInsert<Record, string>(update, x => x.TahoeId, record.TahoeId))
                { IsUpsert = true };
            }));

            await _recordsCollection.BulkWriteAsync(writes, new BulkWriteOptions { IsOrdered = false });
        }
    }
}

