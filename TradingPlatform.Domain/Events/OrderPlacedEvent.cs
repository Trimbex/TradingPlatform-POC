namespace TradingPlatform.Domain.Events;

public record OrderPlacedEvent(
    Guid OrderId,
    string UserId,
    string Symbol,
    decimal Quantity,
    decimal Price
) : DomainEvent;
