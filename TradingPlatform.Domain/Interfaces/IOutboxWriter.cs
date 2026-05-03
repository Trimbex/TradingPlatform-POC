namespace TradingPlatform.Domain.Interfaces;

public interface IOutboxWriter
{
    Task EnqueueAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}
