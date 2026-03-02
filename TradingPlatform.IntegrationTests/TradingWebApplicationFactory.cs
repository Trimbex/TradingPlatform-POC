using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using TradingPlatform.Domain.Interfaces;
using TradingPlatform.Infrastructure.Persistence;

namespace TradingPlatform.IntegrationTests;

/// <summary>
/// In-memory factory for fast, isolated tests. Uses EF InMemory, NoOp UnitOfWork, and TestEventPublisher.
/// </summary>
public class TradingWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseInMemoryDatabase"] = "true",
                ["Kafka:ConsumerEnabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IEventPublisher, TestEventPublisher>();
            services.AddScoped<IUnitOfWork, NoOpUnitOfWork>();
        });
    }
}

/// <summary>
/// Factory that uses real PostgreSQL and Kafka via Testcontainers. Tests full infrastructure stack.
/// </summary>
public class TradingWebApplicationFactoryWithRealInfra : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _postgres;
    private readonly KafkaContainer _kafka;

    public TradingWebApplicationFactoryWithRealInfra()
    {
        _postgres = new PostgreSqlBuilder("postgres:15")
            .WithDatabase("TradingPlatform")
            .WithUsername("tradingplatform")
            .WithPassword("tradingplatform")
            .Build();

        _kafka = new KafkaBuilder("confluentinc/cp-kafka:7.4.1")
            .Build();

        Task.WhenAll(_postgres.StartAsync(), _kafka.StartAsync()).GetAwaiter().GetResult();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var connectionString = _postgres.GetConnectionString();
            var bootstrapServers = _kafka.GetBootstrapAddress();

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseInMemoryDatabase"] = "false",
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Kafka:BootstrapServers"] = bootstrapServers,
                ["Kafka:ConsumerEnabled"] = "false"
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await _kafka.DisposeAsync();
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
