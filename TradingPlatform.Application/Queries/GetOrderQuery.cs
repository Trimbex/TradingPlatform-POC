using MediatR;
using TradingPlatform.Application.DTOs;

namespace TradingPlatform.Application.Queries;

public record GetOrderQuery(Guid OrderId) : IRequest<OrderDto?>;
