namespace DataAggregator.Application.Configurations
{
    public class MongoDbConfiguration
    {
        public string ConnectionString { get; set; } = default!;
        public string DatabaseName { get; set; } = default!;
        public string SummaryCollectionName { get; set; } = "enformion_summaries";
        public string RecordsCollectionName { get; set; } = "enformion_records";
    }
}
