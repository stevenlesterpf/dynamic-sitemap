using DataAggregator.Application.Abstractions;
using DataAggregator.Infrastructure.Clients;

namespace DataAggregator.Infrastructure.Configurations
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddHttpClient<IEnformionClient, EnformionClient>();

            return services;
        }
    }
}
