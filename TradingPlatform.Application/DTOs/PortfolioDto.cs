namespace TradingPlatform.Application.DTOs;

public record PortfolioDto(
    Guid Id,
    string UserId,
    decimal CashBalance,
    IReadOnlyList<HoldingDto> Holdings
);
