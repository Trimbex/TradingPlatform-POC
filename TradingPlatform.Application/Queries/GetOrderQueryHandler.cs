using MediatR;
using TradingPlatform.Application.DTOs;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Queries;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
            return null;

        return MapToDto(order);
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
