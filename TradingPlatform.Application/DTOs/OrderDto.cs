using TradingPlatform.Domain.Enums;

namespace TradingPlatform.Application.DTOs;

public record OrderDto(
    Guid Id,
    string UserId,
    string Symbol,
    decimal Quantity,
    decimal Price,
    OrderStatus Status,
    DateTime CreatedAt
);
