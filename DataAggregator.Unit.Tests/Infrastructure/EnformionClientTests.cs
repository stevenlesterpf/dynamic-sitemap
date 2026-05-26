using DataAggregator.Application.Configurations;
using DataAggregator.Application.Models;
using DataAggregator.Infrastructure.Clients;
using DataAggregator.Unit.Tests.Abstractions;
using DataAggregator.Unit.Tests.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DataAggregator.Unit.Tests.Infrastructure
{
    public class EnformionClientTests : BaseUnitTest<EnformionClientTests, EnformionClient>
    {
        private Mock<HttpMessageHandler> _mockHandler = null!;

        protected override Task SetupClassReference()
        {
            _mockHandler = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("https://api.test.enformion.com")
            };

            var config = Options.Create(new EnformionConfiguration
            {
                BaseUrl = "https://api.test.enformion.com",
                ApiKey = "test-api-key",
                SummaryEndpoint = "/summary",
                RecordsEndpoint = "/records",
                RecordsPageSize = 10,
                TimeoutInSeconds = 30
            });

            _base = new EnformionClient(httpClient, config, new Mock<ILogger<EnformionClient>>().Object);

            return Task.CompletedTask;
        }

        [Fact]
        public void EnformionClient_GetSummaryAsync_Returns_MappedResult()
        {
            Arrange<Empty, EnformionSummaryResult>(_ =>
            {
                var summaryJson = JsonSerializer.Serialize(new
                {
                    content_types = new[]
                    {
                        new { content_type = "person", total_records = 100L, indexable_records = 90L, estimated_sitemaps = 1, latest_modified = "2026-01-01T00:00:00Z" }
                    },
                    totals = new { total_records = 100L, indexable_records = 90L, estimated_sitemaps = 1 },
                    data_freshness = new { last_sync = "2026-01-01T00:00:00Z", next_sync = "2026-01-02T00:00:00Z" }
                });

                _mockHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(summaryJson, Encoding.UTF8, "application/json")
                    });
            })
            .Act((client, request) => client.GetSummaryAsync().GetAwaiter().GetResult())
            .Assert(result =>
            {
                result.Should().NotBeNull();
                result.Totals.TotalRecords.Should().Be(100);
                result.ContentTypes.Should().HaveCount(1);
                result.ContentTypes[0].ContentType.Should().Be("person");
            });
        }

        [Fact]
        public void EnformionClient_GetRecordsAsync_SinglePage_ReturnsAllRecords()
        {
            Arrange<GetRecordsRequest, List<EnformionRecordResult>>(request =>
            {
                request.ContentType = "person";
                request.PageSize = 10;
                var recordsJson = JsonSerializer.Serialize(new
                {
                    data = new[]
                    {
                        new { tahoe_id = "id1", content_type = "person", attrs = new { }, last_modified = "2026-01-01T00:00:00Z", change_frequency = "weekly", priority = 0.5 },
                        new { tahoe_id = "id2", content_type = "person", attrs = new { }, last_modified = "2026-01-01T00:00:00Z", change_frequency = "monthly", priority = 0.3 }
                    },
                    pagination = new { total = 2L, limit = 10, offset = 0, has_more = false, next_cursor = "" }
                });

                _mockHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(recordsJson, Encoding.UTF8, "application/json")
                    });
            })
            .Act((client, request) =>
            {
                var records = new List<EnformionRecordResult>();
                var enumerator = client.GetRecordsAsync(request.ContentType, request.PageSize).GetAsyncEnumerator();
                try
                {
                    while (enumerator.MoveNextAsync().GetAwaiter().GetResult())
                        records.Add(enumerator.Current);
                }
                finally
                {
                    enumerator.DisposeAsync().GetAwaiter().GetResult();
                }
                return records;
            })
            .Assert(records =>
            {
                records.Should().HaveCount(2);
                records[0].Id.Should().Be("id1");
                records[1].Id.Should().Be("id2");
            });
        }

        [Fact]
        public void EnformionClient_GetRecordsAsync_MultiPage_FollowsCursorUntilExhausted()
        {
            Arrange<GetRecordsRequest, List<EnformionRecordResult>>(request =>
            {
                request.ContentType = "person";
                request.PageSize = 1;
                var page1Json = JsonSerializer.Serialize(new
                {
                    data = new[] { new { tahoe_id = "id1", content_type = "person", attrs = new { }, last_modified = "2026-01-01T00:00:00Z", change_frequency = "weekly", priority = 0.5 } },
                    pagination = new { total = 2L, limit = 1, offset = 0, has_more = true, next_cursor = "cursor123" }
                });

                var page2Json = JsonSerializer.Serialize(new
                {
                    data = new[] { new { tahoe_id = "id2", content_type = "person", attrs = new { }, last_modified = "2026-01-01T00:00:00Z", change_frequency = "monthly", priority = 0.3 } },
                    pagination = new { total = 2L, limit = 1, offset = 1, has_more = false, next_cursor = "" }
                });

                _mockHandler.Protected()
                    .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(page1Json, Encoding.UTF8, "application/json")
                    })
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(page2Json, Encoding.UTF8, "application/json")
                    });
            }, expected =>
            {
                expected.Add(new EnformionRecordResult
                {
                    Id = "id1",
                    ContentType = "person",
                    ChangeFrequency = "weekly",
                    Priority = 0.5m,
                    LastModified = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Attributes = []
                });
                expected.Add(new EnformionRecordResult
                {
                    Id = "id2",
                    ContentType = "person",
                    ChangeFrequency = "monthly",
                    Priority = 0.3m,
                    LastModified = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Attributes = []
                });
            })
            .Act((client, request) =>
            {
                var records = new List<EnformionRecordResult>();
                var enumerator = client.GetRecordsAsync(request.ContentType, request.PageSize).GetAsyncEnumerator();
                try
                {
                    while (enumerator.MoveNextAsync().GetAwaiter().GetResult())
                        records.Add(enumerator.Current);
                }
                finally
                {
                    enumerator.DisposeAsync().GetAwaiter().GetResult();
                }
                return records;
            })
            .Assert(records =>
            {
                records.Should().HaveCount(2);
                records[0].Id.Should().Be("id1");
                records[1].Id.Should().Be("id2");
                records[0].Priority.Should().Be(0.5m);
                records[1].Priority.Should().Be(0.3m);
            });
        }

        [Fact]
        public void EnformionClient_GetSummaryAsync_WhenHttpReturnsError_ThrowsHttpRequestException()
        {
            Arrange(() =>
            {
                _mockHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            })
            .Act(client => client.GetSummaryAsync().GetAwaiter().GetResult())
            .AssertThrows<HttpRequestException>(exception =>
            {
                exception.Message.Should().Contain("Internal Server Error");
            });
            
        }

        [Fact]
        public void EnformionClient_GetRecordsAsync_WhenHttpReturnsError_ThrowsHttpRequestException()
        {
            Arrange(() =>
            {
                _mockHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            })
            .Act(client =>
            {
                var enumerator = client.GetRecordsAsync("person", 10).GetAsyncEnumerator();
                enumerator.MoveNextAsync().GetAwaiter().GetResult();
            }).AssertThrows<HttpRequestException>(exception =>
            {
                exception.Message.Should().Contain("Service Unavailable");
            });
        }
    }
}

