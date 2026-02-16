using MediatR;
using TradingPlatform.Application.DTOs;

namespace TradingPlatform.Application.Queries;

public record GetPortfolioQuery(string UserId) : IRequest<PortfolioDto?>;
