using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingPlatform.Domain.Events;
using TradingPlatform.Domain.Interfaces;
using TradingPlatform.Infrastructure.Persistence;

namespace TradingPlatform.Infrastructure.Messaging;

public class OutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected outbox dispatch error");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var pending = await context.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
            return;

        foreach (var message in pending)
        {
            try
            {
                var domainEvent = Deserialize(message);
                if (domainEvent is null)
                {
                    message.RetryCount++;
                    message.Error = $"Unsupported outbox type '{message.Type}'";
                    continue;
                }

                await PublishEventAsync(publisher, domainEvent, cancellationToken);
                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static object? Deserialize(OutboxMessage message)
    {
        return message.Type switch
        {
            "TradingPlatform.Domain.Events.OrderPlacedEvent" =>
                System.Text.Json.JsonSerializer.Deserialize<OrderPlacedEvent>(message.Payload),
            "TradingPlatform.Domain.Events.OrderCancelledEvent" =>
                System.Text.Json.JsonSerializer.Deserialize<OrderCancelledEvent>(message.Payload),
            "TradingPlatform.Domain.Events.OrderExecutedEvent" =>
                System.Text.Json.JsonSerializer.Deserialize<OrderExecutedEvent>(message.Payload),
            "TradingPlatform.Domain.Events.FundsDepositedEvent" =>
                System.Text.Json.JsonSerializer.Deserialize<FundsDepositedEvent>(message.Payload),
            _ => null
        };
    }

    private static Task PublishEventAsync(IEventPublisher publisher, object domainEvent, CancellationToken cancellationToken)
    {
        return domainEvent switch
        {
            OrderPlacedEvent e => publisher.PublishAsync(e, cancellationToken),
            OrderCancelledEvent e => publisher.PublishAsync(e, cancellationToken),
            OrderExecutedEvent e => publisher.PublishAsync(e, cancellationToken),
            FundsDepositedEvent e => publisher.PublishAsync(e, cancellationToken),
            _ => Task.CompletedTask
        };
    }
}
