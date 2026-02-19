using MediatR;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Commands;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Unit> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order {request.OrderId} not found.");

        order.Cancel();
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Unit.Value;
    }
}
