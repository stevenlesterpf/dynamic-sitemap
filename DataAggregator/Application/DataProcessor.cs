using DataAggregator.Application.Abstractions;
using DataAggregator.Infrastructure.Services;

namespace DataAggregator.Application
{
    public class DataProcessor
    {
        private readonly IEnformionClient _enformionClient;
        private readonly SnsPublisher _snsPublisher;

        public DataProcessor(IEnformionClient enformionClient, SnsPublisher snsPublisher)
        {
            _enformionClient = enformionClient;
            _snsPublisher = snsPublisher;
        }

        public async Task ProcessAsync()
        {

        }
    }
}
