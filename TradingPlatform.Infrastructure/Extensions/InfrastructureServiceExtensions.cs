using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradingPlatform.Domain.Interfaces;
using TradingPlatform.Infrastructure.Messaging;
using TradingPlatform.Infrastructure.Persistence;
using TradingPlatform.Infrastructure.Persistence.Repositories;

namespace TradingPlatform.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TradingDbContext>(options =>
        {
            if (configuration["UseInMemoryDatabase"] == "true")
            {
                options.UseInMemoryDatabase("TradingPlatform");
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? "Host=localhost;Port=5432;Database=TradingPlatform;Username=tradingplatform;Password=tradingplatform";
                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        return services;
    }
}
