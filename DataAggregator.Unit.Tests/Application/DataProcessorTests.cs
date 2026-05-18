using DataAggregator.Application;
using DataAggregator.Application.Abstractions;
using DataAggregator.Infrastructure.Services;
using DataAggregator.Unit.Tests.Abstractions;
using Moq;

namespace DataAggregator.Unit.Tests.Application
{
    public class DataProcessorTests : BaseUnitTest<DataProcessorTests, DataProcessor>
    {
        protected override void SetupClassReference()
        {
            var client = new Mock<IEnformionClient>();
            var publisher = new Mock<SnsPublisher>();

            _base = new DataProcessor(client.Object, publisher.Object);
        }

        protected override void ActProcessor()
        {
            _base
                .ProcessAsync()
                .GetAwaiter()
                .GetResult();
        }

        [Fact]
        public void DataProcessor_ProcessAsync_Runs_Successfully()
        {
            Arrange()
            .Act()
            .Assert();
        }
    }
}
