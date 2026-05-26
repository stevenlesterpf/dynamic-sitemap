using DataAggregator.Application;
using DataAggregator.Application.Abstractions;
using DataAggregator.Application.Configurations;
using DataAggregator.Application.Mappers;
using DataAggregator.Infrastructure.Clients;
using DataAggregator.Infrastructure.Persistence;
using DataAggregator.Infrastructure.Services;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using System.Net.Http.Headers;

namespace DataAggregator.Infrastructure.Configurations
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EnformionConfiguration>(config.GetSection(nameof(EnformionConfiguration)));
            services.Configure<MongoDbConfiguration>(config.GetSection(nameof(MongoDbConfiguration)));

            services.AddSingleton<IMongoClient>(sp =>
            {
                var mongoConfig = sp.GetRequiredService<IOptions<MongoDbConfiguration>>().Value;
                return new MongoClient(mongoConfig.ConnectionString);
            });

            services.AddSingleton<IMongoDb, MongoDb>();
            services.AddSingleton<EnformionMapper>();
            services.AddSingleton<SnsPublisher>();
            services.AddSingleton<DataProcessor>();

            services
                .AddHttpClient<IEnformionClient, EnformionClient>((sp, client) =>
                {
                    var enformionConfig = sp.GetRequiredService<IOptions<EnformionConfiguration>>().Value;
                    client.BaseAddress = new Uri(enformionConfig.BaseUrl);
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", enformionConfig.ApiKey);
                    client.Timeout = TimeSpan.FromSeconds(enformionConfig.TimeoutInSeconds);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                    MaxConnectionsPerServer = 10
                })
                .AddResilienceHandler("enformion-retry", (builder, context) =>
                {
                    var enformionConfig = context.ServiceProvider
                        .GetRequiredService<IOptions<EnformionConfiguration>>().Value;

                    builder.AddRetry(new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = enformionConfig.MaxRetries,
                        Delay = TimeSpan.FromSeconds(enformionConfig.RetryDelayInSeconds),
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        ShouldHandle = static args => ValueTask.FromResult(
                            args.Outcome.Exception is HttpRequestException ||
                            args.Outcome.Result?.StatusCode is
                                System.Net.HttpStatusCode.TooManyRequests or
                                System.Net.HttpStatusCode.ServiceUnavailable or
                                System.Net.HttpStatusCode.GatewayTimeout)
                    });
                });

            return services;
        }
    }
}

