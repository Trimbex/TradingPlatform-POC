namespace TradingPlatform.Domain.Events;

public record OrderCancelledEvent(Guid OrderId) : DomainEvent;
