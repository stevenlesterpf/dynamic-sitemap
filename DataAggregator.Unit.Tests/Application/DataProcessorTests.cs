using DataAggregator.Application;
using DataAggregator.Application.Abstractions;
using DataAggregator.Application.Configurations;
using DataAggregator.Application.Mappers;
using DataAggregator.Application.Models;
using DataAggregator.Infrastructure.Services;
using DataAggregator.Unit.Tests.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Record = DataAggregator.Application.Models.Record;

namespace DataAggregator.Unit.Tests.Application
{
    public class DataProcessorTests : BaseUnitTest<DataProcessorTests, DataProcessor>
    {
        private Mock<IEnformionClient> _mockClient = null!;
        private Mock<IMongoDb> _mockMongoDb = null!;

        protected override Task SetupClassReference()
        {
            _mockClient = new Mock<IEnformionClient>();
            _mockMongoDb = new Mock<IMongoDb>();

            _mockClient
                .Setup(x => x.GetSummaryAsync())
                .ReturnsAsync(new EnformionSummaryResult
                {
                    ContentTypes = [],
                    Totals = new TotalSummary { TotalRecords = 0 },
                    DataFreshness = new DataFreshness { LastSync = DateTime.UtcNow }
                });

            _mockClient
                .Setup(x => x.GetRecordsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns<string, int, string?>((_, _, _) => EmptyRecordsAsync());

            _mockMongoDb
                .Setup(x => x.EnsureIndexesAsync())
                .Returns(Task.CompletedTask);

            var publisherLogger = new Mock<ILogger<SnsPublisher>>();
            var publisher = new SnsPublisher(publisherLogger.Object);
            var processorLogger = new Mock<ILogger<DataProcessor>>();
            var mapper = new EnformionMapper();
            var config = Options.Create(new EnformionConfiguration { RecordsPageSize = 100 });

            _base = new DataProcessor(
                _mockClient.Object,
                publisher,
                _mockMongoDb.Object,
                mapper,
                processorLogger.Object,
                config);

            return Task.CompletedTask;
        }

        [Fact]
        public void DataProcessor_ProcessAsync_Runs_Successfully()
        {
            Arrange()
            .Act(processor => processor
                .ProcessAsync()
                .GetAwaiter()
                .GetResult())
            .Assert();
        }

        [Fact]
        public void DataProcessor_ProcessAsync_FetchesSummaryFromEnformion()
        {
            Arrange()
            .Act(processor =>

                processor.ProcessAsync()
                    .GetAwaiter()
                    .GetResult()

            )
            .Assert(() => 
                _mockClient.Verify(
                    x => x.GetSummaryAsync(),
                    Times.Once)
            );
        }

        [Fact]
        public void DataProcessor_ProcessAsync_PersistsSummaryToMongo()
        {
            Arrange()
            .Act(processor =>

                processor
                    .ProcessAsync()
                    .GetAwaiter()
                    .GetResult()
            )
            .Assert(() =>
                _mockMongoDb.Verify(
                    x => x.SaveSummaryAsync(It.IsAny<Summary>()),
                    Times.Once)
            );
        }

        [Fact]
        public void DataProcessor_ProcessAsync_WithContentTypes_FetchesRecordsForEachType()
        {
            Arrange(() => {
                _mockClient
                    .Setup(x => x.GetSummaryAsync())
                    .ReturnsAsync(new EnformionSummaryResult
                    {
                        ContentTypes =
                        [
                            new ContentTypeSummary { ContentType = "person", TotalRecords = 10 },
                        new ContentTypeSummary { ContentType = "phone", TotalRecords = 5 }
                        ],
                        Totals = new TotalSummary { TotalRecords = 15 },
                        DataFreshness = new DataFreshness { LastSync = DateTime.UtcNow }
                    });
            })
            .Act(processor =>

                processor.ProcessAsync()
                    .GetAwaiter()
                    .GetResult()
             )
            .Assert(() =>
            {
                _mockClient.Verify(
                    x => x.GetRecordsAsync("person", It.IsAny<int>(), It.IsAny<string?>()),
                    Times.Once);

                _mockClient.Verify(
                    x => x.GetRecordsAsync("phone", It.IsAny<int>(), It.IsAny<string?>()),
                    Times.Once);
            });
        }

        [Fact]
        public void DataProcessor_ProcessAsync_WithRecords_PersistsThemToMongo()
        {
            Arrange(() => {
                _mockClient
                    .Setup(x => x.GetSummaryAsync())
                    .ReturnsAsync(new EnformionSummaryResult
                    {
                        ContentTypes = [new ContentTypeSummary { ContentType = "person", TotalRecords = 2 }],
                        Totals = new TotalSummary { TotalRecords = 2 },
                        DataFreshness = new DataFreshness { LastSync = DateTime.UtcNow }
                    });

                _mockClient
                    .Setup(x => x.GetRecordsAsync("person", It.IsAny<int>(), It.IsAny<string?>()))
                    .Returns<string, int, string?>((_, _, _) => TwoRecordsAsync());
            })
            .Act(processor =>

                processor
                    .ProcessAsync()
                    .GetAwaiter()
                    .GetResult()
            )
            .Assert(() =>
            {
                _mockMongoDb.Verify(
                    x => x.SaveRecordsAsync(
                        It.Is<IReadOnlyList<Record>>(r => r.Count == 2)),
                    Times.Once);
            });
        }

        [Fact]
        public void DataProcessor_ProcessAsync_WhenClientThrowsOnGetSummary_PropagatesException()
        {
            Arrange(() =>
            {
                _mockClient
                    .Setup(x => x.GetSummaryAsync())
                    .ThrowsAsync(new HttpRequestException("Service unavailable"));
            })
            .Act(processor =>
                processor.ProcessAsync()
                    .GetAwaiter()
                    .GetResult()
            ).AssertThrows<HttpRequestException>(exception =>
            {
                exception.Message.Should().Contain("Service unavailable");
            });
        }

        [Fact]
        public void DataProcessor_ProcessAsync_WhenMongoDbThrowsOnSaveSummary_PropagatesException()
        {
            Arrange(() =>
            {
                _mockMongoDb
                    .Setup(x => x.SaveSummaryAsync(It.IsAny<Summary>()))
                    .ThrowsAsync(new InvalidOperationException("Database write failed"));
            })
            .Act(processor =>
                processor.ProcessAsync()
                    .GetAwaiter()
                    .GetResult()
            ).AssertThrows<InvalidOperationException>(exception =>
            {
                exception.Message.Should().Contain("Database write failed");
            });
        }

        private static async IAsyncEnumerable<EnformionRecordResult> EmptyRecordsAsync()
        {
            yield break;
        }

        private static async IAsyncEnumerable<EnformionRecordResult> TwoRecordsAsync()
        {
            yield return new EnformionRecordResult { Id = "id1", ContentType = "person", Attributes = new(), LastModified = DateTime.UtcNow, ChangeFrequency = "weekly", Priority = 0.5m };
            yield return new EnformionRecordResult { Id = "id2", ContentType = "person", Attributes = new(), LastModified = DateTime.UtcNow, ChangeFrequency = "monthly", Priority = 0.3m };
        }
    }
}

