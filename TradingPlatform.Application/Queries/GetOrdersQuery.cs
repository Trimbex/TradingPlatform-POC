using MediatR;
using TradingPlatform.Application.DTOs;

namespace TradingPlatform.Application.Queries;

public record GetOrdersQuery(string UserId) : IRequest<IReadOnlyList<OrderDto>>;
