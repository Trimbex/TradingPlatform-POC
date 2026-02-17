namespace TradingPlatform.Domain.Events;

public record OrderExecutedEvent(Guid OrderId) : DomainEvent;
