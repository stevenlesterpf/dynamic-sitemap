using DataAggregator.Application.Configurations;
using DataAggregator.Application.Helper;
using DataAggregator.Application.Models;
using DataAggregator.Unit.Tests.Abstractions;
using FluentAssertions;

namespace DataAggregator.Unit.Tests.Application
{
    public class DataMapperTests : BaseUnitTest<DataMapperTests, DataMapper>
    {
        protected override Task SetupClassReference()
        {
            _base = new DataMapper();
            return Task.CompletedTask;
        }

        [Fact]
        public void DataMapper_Transform_Runs_Successfully()
        {
            Arrange<AwsConfiguration, CollectionPattern>(
                request =>
                {
                    request.Test = string.Empty;
                }
            )
            .Act((mapper, request) => mapper.Transform(request))
            .Assert(result => 
                result
                    .Should()
                    .BeNull());
        }
    }
}
