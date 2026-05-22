using DataAggregator.Application.Mappers;
using DataAggregator.Application.Models;
using DataAggregator.Unit.Tests.Abstractions;
using FluentAssertions;
using Record = DataAggregator.Application.Models.Record;

namespace DataAggregator.Unit.Tests.Application
{
    public class EnformionMapperTests : BaseUnitTest<EnformionMapperTests, EnformionMapper>
    {
        protected override Task SetupClassReference()
        {
            _base = new EnformionMapper();
            return Task.CompletedTask;
        }

        [Fact]
        public void EnformionMapper_MapToPersistedSummary_MapsAllFields()
        {
            var fetchedAt = DateTime.UtcNow;

            Arrange<EnformionSummaryResult, Summary>(request =>
            {
                request.ContentTypes =
                [
                    new ContentTypeSummary
                    {
                        ContentType = "person",
                        TotalRecords = 50,
                        IndexableRecords = 20,
                        EstimatedSitemaps = 1,
                        LatestModified = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    }
                ];
                request.Totals = new TotalSummary { TotalRecords = 100, IndexableRecords = 90, EstimatedSitemaps = 1 };
                request.DataFreshness = new DataFreshness
                {
                    LastSync = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    NextSync = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                };
            }, expected =>
            {
                expected.FetchedAt = fetchedAt;

                expected.Totals = new PersistedTotalSummary
                {
                    TotalRecords = 100,
                    IndexableRecords = 90,
                    EstimatedSitemaps = 1
                };

                expected.ContentTypes =
                [
                    new PersistedContentTypeSummary
                    {
                        ContentType = "person",
                        TotalRecords = 50,
                        IndexableRecords = 20,
                        EstimatedSitemaps = 1,
                        LatestModified = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    }
                ];

                expected.DataFreshness = new PersistedDataFreshness
                {
                    LastSync = new DateTime(
                        2026, 1, 1, 0, 0, 0,
                        DateTimeKind.Utc),

                    NextSync = new DateTime(
                        2026, 1, 2, 0, 0, 0,
                        DateTimeKind.Utc)
                };
            })
            .Act((mapper, request) => mapper.MapToPersistedSummary(request, fetchedAt))
            .Assert(result =>
            {
                result.Should().NotBeNull();

                result.FetchedAt.Should().Be(fetchedAt);

                result.Totals.TotalRecords.Should().Be(100);

                result.ContentTypes.Should().HaveCount(1);

                result.ContentTypes[0]
                    .ContentType
                    .Should()
                    .Be("person");

                result.DataFreshness.NextSync.Should().Be(
                    new DateTime(
                        2026, 1, 2, 0, 0, 0,
                        DateTimeKind.Utc));
            });
        }

        [Fact]
        public void EnformionMapper_MapToPersistedSummary_WithEmptyContentTypes_ReturnsEmptyList()
        {
            var fetchedAt = DateTime.UtcNow;
            Arrange<EnformionSummaryResult, Summary>(request =>
            {
                request.ContentTypes = [];
                request.Totals = new TotalSummary();
                request.DataFreshness = new DataFreshness();

            }, expected =>
            {
                expected.ContentTypes = [];
                expected.Totals = new PersistedTotalSummary();
                expected.DataFreshness = new PersistedDataFreshness();
                expected.FetchedAt = fetchedAt;
            })
            .Act((mapper, request) => mapper.MapToPersistedSummary(request, fetchedAt))
            .Assert(result =>
            {
                result.ContentTypes.Should().BeEmpty();
                result.Totals.Should().BeEquivalentTo(new PersistedTotalSummary());
                result.DataFreshness.Should().BeEquivalentTo(new PersistedDataFreshness());
                result.FetchedAt.Should().Be(fetchedAt);
            });
        }

        [Fact]
        public void EnformionMapper_MapToPersistedRecord_MapsAllFields()
        {
            var persistedAt = DateTime.UtcNow;

            Arrange<EnformionRecordResult, Record>(request =>
            {
                request.Id = "tahoe-123";
                request.ContentType = "person";
                request.Attributes = new Dictionary<string, string?> { ["first_name"] = "John" };
                request.LastModified = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                request.ChangeFrequency = "weekly";
                request.Priority = 0.8m;

            }, expected =>
            {
                expected.TahoeId = "tahoe-123";
                expected.ContentType = "person";
                expected.Person = new ContentTypeData
                {
                    Attributes = new Dictionary<string, string?>
                    {
                        ["first_name"] = "John"
                    },
                    LastModified = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ChangeFrequency = "weekly",
                    Priority = 0.8m,
                    PersistedAt = persistedAt,
                };
            })
            .Act((mapper, request) => mapper.MapToPersistedRecord(request, persistedAt))
            .Assert(result =>
            {
                result.Should().NotBeNull();
                result.ContentType.Should().Be("person");
                result.TahoeId.Should().Be("tahoe-123");
                result.Person.Should().NotBeNull();
                result.Person!.ChangeFrequency.Should().Be("weekly");
                result.Person.Priority.Should().Be(0.8m);
                result.Person.PersistedAt.Should().Be(persistedAt);
                result.Person.Attributes.Should().ContainKey("first_name");
            });
        }

        [Fact]
        public void EnformionMapper_MapToPersistedRecord_WithNullAttributes_ReturnsEmptyDictionary()
        {
            var persistedAt = DateTime.UtcNow;
            Arrange<EnformionRecordResult, Record>(request =>
            {
                request.Id = "tahoe-456";
                request.ContentType = "phone";
                request.Attributes = null!;

            }, expected => {

                expected.ContentType = "phone";
                expected.Phone = new ContentTypeData
                {
                    Attributes = [],
                    PersistedAt = persistedAt,
                };
                expected.TahoeId = "tahoe-456";

            })
            .Act((mapper, request) => mapper.MapToPersistedRecord(request, persistedAt))
            .Assert(result =>
            {
                result.Phone!.Attributes.Should().NotBeNull().And.BeEmpty();
                result.Phone.Should().NotBeNull();
                result.ContentType.Should().Be("phone");
                result.TahoeId.Should().Be("tahoe-456");
                result.Phone.PersistedAt.Should().Be(persistedAt);
            });
        }

        [Fact]
        public void EnformionMapper_MapToPersistedRecord_WithUnknownContentType_ThrowsArgumentOutOfRangeException()
        {
            var persistedAt = DateTime.UtcNow;

            Arrange<EnformionRecordResult, Record>(request =>
            {
                request.Id = "tahoe-999";
                request.ContentType = "unknown-type";
                request.Attributes = new Dictionary<string, string?>();
            })
            .Act((mapper, request) => mapper.MapToPersistedRecord(request, persistedAt))
            .AssertThrows<ArgumentOutOfRangeException>(ex =>
            {
                ex.Message.Should().Contain("unknown-type");
            });
        }

        [Fact]
        public void EnformionMapper_MapToPersistedSummary_WithEmptyTotals_MapsZeroValues()
        {
            var fetchedAt = DateTime.UtcNow;

            Arrange<EnformionSummaryResult, Summary>(request =>
            {
                request.ContentTypes = [];
                request.Totals = new TotalSummary { TotalRecords = 0, IndexableRecords = 0, EstimatedSitemaps = 0 };
                request.DataFreshness = new DataFreshness
                {
                    LastSync = default,
                    NextSync = default
                };
            }, expected =>
            {
                expected.FetchedAt = fetchedAt;
                expected.ContentTypes = [];
                expected.Totals = new PersistedTotalSummary { TotalRecords = 0, IndexableRecords = 0, EstimatedSitemaps = 0 };
                expected.DataFreshness = new PersistedDataFreshness { LastSync = default, NextSync = default };
            })
            .Act((mapper, request) => mapper.MapToPersistedSummary(request, fetchedAt))
            .Assert(result =>
            {
                result.Should().NotBeNull();
                result.Totals.TotalRecords.Should().Be(0);
                result.Totals.IndexableRecords.Should().Be(0);
                result.ContentTypes.Should().BeEmpty();
            });
        }
    }
}
