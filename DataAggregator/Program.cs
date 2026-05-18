using DataAggregator;

var builder = Host.CreateApplicationBuilder(args);
DependencyInjection.ConfigureServices(builder.Services, builder.Configuration);

var host = builder.Build();
host.Run();
