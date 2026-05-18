using DataAggregator.Application;
using DataAggregator.Application.Abstractions;
using DataAggregator.Infrastructure.Services;
using DataAggregator.Unit.Tests.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataAggregator.Unit.Tests.Application
{
    public class DataProcessorTests : BaseUnitTest<DataProcessorTests, DataProcessor>
    {
        protected override Task SetupClassReference()
        {
            var client = new Mock<IEnformionClient>();
            var publisherLogger = new Mock<ILogger<SnsPublisher>>();
            var publisher = new SnsPublisher(publisherLogger.Object);

            _base = new DataProcessor(client.Object, publisher);
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
    }
}
