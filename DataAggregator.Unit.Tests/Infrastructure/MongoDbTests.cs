using DataAggregator.Application.Configurations;
using DataAggregator.Application.Models;
using DataAggregator.Infrastructure.Persistence;
using DataAggregator.Unit.Tests.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using MongoDB.Driver;
using Record = DataAggregator.Application.Models.Record;

namespace DataAggregator.Unit.Tests.Infrastructure
{
    public class MongoDbTests : BaseUnitTest<MongoDbTests, MongoDb>
    {
        private Mock<IMongoCollection<Summary>> _mockSummaryCollection = null!;
        private Mock<IMongoCollection<Record>> _mockRecordsCollection = null!;

        protected override Task SetupClassReference()
        {
            _mockSummaryCollection = new Mock<IMongoCollection<Summary>>();
            _mockRecordsCollection = new Mock<IMongoCollection<Record>>();

            var mockDatabase = new Mock<IMongoDatabase>();

            mockDatabase
                .Setup(x => x.GetCollection<Summary>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings?>()))
                .Returns(_mockSummaryCollection.Object);

            mockDatabase
                .Setup(x => x.GetCollection<Record>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings?>()))
                .Returns(_mockRecordsCollection.Object);

            var mockClient = new Mock<IMongoClient>();
            mockClient
                .Setup(x => x.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings?>()))
                .Returns(mockDatabase.Object);

            var config = Options.Create(new MongoDbConfiguration
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "test_db",
                SummaryCollectionName = "enformion_summaries",
                RecordsCollectionName = "enformion_records"
            });

            _base = new MongoDb(mockClient.Object, config);

            return Task.CompletedTask;
        }

        [Fact]
        public void MongoDb_SaveSummaryAsync_CallsInsertOne()
        {
            Arrange(() =>
            {
                _mockSummaryCollection
                    .Setup(x => x.InsertOneAsync(It.IsAny<Summary>(), It.IsAny<InsertOneOptions?>()))
                    .Returns(Task.CompletedTask);
            })
            .Act((db) =>
            {
                db.SaveSummaryAsync(new Summary
                {
                    ContentTypes = [],
                    Totals = new PersistedTotalSummary(),
                    DataFreshness = new PersistedDataFreshness(),
                    FetchedAt = DateTime.UtcNow
                }).GetAwaiter().GetResult();
              
            })
            .Assert(() =>
            {
                _mockSummaryCollection.Verify(
                    x => x.InsertOneAsync(It.IsAny<Summary>(), It.IsAny<InsertOneOptions?>()),
                    Times.Once);
            });
        }

        [Fact]
        public void MongoDb_EnsureIndexesAsync_CreatesUniqueIndexOnTahoeId()
        {
            var mockIndexManager = new Mock<IMongoIndexManager<Record>>();

            Arrange(() =>
            {
                mockIndexManager
                    .Setup(x => x.CreateOneAsync(
                        It.IsAny<CreateIndexModel<Record>>(),
                        It.IsAny<CreateOneIndexOptions?>()))
                    .ReturnsAsync("idx_tahoe_id_unique");

                _mockRecordsCollection
                    .Setup(x => x.Indexes)
                    .Returns(mockIndexManager.Object);
            })
            .Act((db) =>
                db.EnsureIndexesAsync().GetAwaiter().GetResult()
            )
            .Assert(() =>
            {
                mockIndexManager.Verify(
                    x => x.CreateOneAsync(
                        It.Is<CreateIndexModel<Record>>(m => m.Options != null && m.Options.Unique == true),
                        It.IsAny<CreateOneIndexOptions?>()),
                    Times.Once);
            });
        }

        [Fact]
        public void MongoDb_SaveRecordsAsync_WithRecords_CallsBulkWrite()
        {
            Arrange(() =>
            {
                _mockRecordsCollection
                    .Setup(x => x.BulkWriteAsync(
                        It.IsAny<IEnumerable<WriteModel<Record>>>(),
                        It.IsAny<BulkWriteOptions?>()
                    ))
                    .ReturnsAsync((BulkWriteResult<Record>)null!);
            })
            .Act((db) =>
            {
                db.SaveRecordsAsync(new List<Record>
                {
                    new() { TahoeId = "id1", ContentType = "person",  Person  = new ContentTypeData() },
                    new() { TahoeId = "id2", ContentType = "address", Address = new ContentTypeData() }
                }).GetAwaiter().GetResult();
            })
            .Assert(() =>
            {
                _mockRecordsCollection.Verify(
                    x => x.BulkWriteAsync(
                        It.Is<IEnumerable<WriteModel<Record>>>(w => w.Count() == 2),
                        It.IsAny<BulkWriteOptions?>()
                    ),
                    Times.Once);
            });
        }

        [Fact]
        public void MongoDb_SaveRecordsAsync_WithEmptyList_DoesNotCallDatabase()
        {
            Arrange()
            .Act((db) =>
            {
                db.SaveRecordsAsync([]).GetAwaiter().GetResult();
            })
            .Assert(() =>
            {
                _mockRecordsCollection.Verify(
                    x => x.BulkWriteAsync(
                        It.IsAny<IEnumerable<WriteModel<Record>>>(),
                        It.IsAny<BulkWriteOptions?>()),
                    Times.Never);
            });
        }
    }
}

