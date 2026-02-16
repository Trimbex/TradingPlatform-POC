namespace TradingPlatform.Application.DTOs;

public record HoldingDto(
    string Symbol,
    decimal Quantity,
    decimal AveragePrice
);
