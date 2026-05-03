using System.Text.Json;
using TradingPlatform.Domain.Interfaces;
using TradingPlatform.Infrastructure.Persistence;

namespace TradingPlatform.Infrastructure.Messaging;

public class OutboxWriter : IOutboxWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly TradingDbContext _context;

    public OutboxWriter(TradingDbContext context)
    { 
        _context = context;
    }

    public async Task EnqueueAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(T).FullName ?? typeof(T).Name;
        var payload = JsonSerializer.Serialize(@event, JsonOptions);

        var now = DateTime.UtcNow;
        await _context.OutboxMessages.AddAsync(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = eventType,
            Payload = payload,
            OccurredOnUtc = now,
            NextAttemptAtUtc = now
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
