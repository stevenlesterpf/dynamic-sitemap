using DataAggregator.Infrastructure.Configurations;
using DataAggregator.Worker;

namespace DataAggregator
{
    public class DependencyInjection
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddHostedService<DataWorker>();
            services.AddInfrastructure(config);
        }
    }
}
