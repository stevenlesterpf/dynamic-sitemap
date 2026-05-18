using DataAggregator.Application;

namespace DataAggregator.Worker
{
    public class DataWorker : BackgroundService
    {
        private readonly ILogger<DataWorker> _logger;
        private readonly DataProcessor _dataProcessor;

        public DataWorker(ILogger<DataWorker> logger, DataProcessor dataProcessor)
        {
            _logger = logger;
            _dataProcessor = dataProcessor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _dataProcessor.ProcessAsync();
        }
    }
}
