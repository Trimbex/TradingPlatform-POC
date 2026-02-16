using MediatR;
using TradingPlatform.Application.DTOs;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Queries;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByUserAsync(request.UserId, cancellationToken);

        return orders
            .Select(MapToDto)
            .ToList();
    }

    private static OrderDto MapToDto(Order order) =>
        new(
            order.Id,
            order.UserId,
            order.Symbol,
            order.Quantity,
            order.Price,
            order.Status,
            order.CreatedAt
        );
}
