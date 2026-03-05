namespace TradingPlatform.Application.DTOs;

public record TransactionDto(
    Guid Id,
    string UserId,
    Guid? OrderId,
    string Type,
    decimal Amount,
    DateTime Timestamp,
    string Status
);

