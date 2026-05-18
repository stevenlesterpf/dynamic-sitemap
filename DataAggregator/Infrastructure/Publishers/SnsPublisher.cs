namespace DataAggregator.Infrastructure.Services
{
    public class SnsPublisher
    {
        private readonly ILogger<SnsPublisher> _logger;

        public SnsPublisher(ILogger<SnsPublisher> logger)
        {
            _logger = logger;
        }

        public async Task PublishAsync<T>(T message)
        {
            
        }
    }
}
