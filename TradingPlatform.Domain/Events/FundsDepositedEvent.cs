namespace TradingPlatform.Domain.Events;

public record FundsDepositedEvent(
    string UserId,
    decimal Amount
) : DomainEvent;
