using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.IntegrationTests;

/// <summary>
/// No-op event publisher for integration tests. Prevents Kafka connection attempts.
/// </summary>
public class TestEventPublisher : IEventPublisher
{ // temu kafka
    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
        => Task.CompletedTask;
}
