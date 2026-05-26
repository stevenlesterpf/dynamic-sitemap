namespace DataAggregator.Application.Configurations
{
    public class EnformionConfiguration
    {
        public string BaseUrl { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string SummaryEndpoint { get; set; } = "/v1/sitemap/summary";
        public string RecordsEndpoint { get; set; } = "/v1/sitemap/records";
        public int RecordsPageSize { get; set; } = 1000;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelayInSeconds { get; set; } = 1;
        public int TimeoutInSeconds { get; set; } = 300;
    }
}
