using MediatR;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Events;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Commands;

public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;

    public PlaceOrderCommandHandler(IOrderRepository orderRepository, IOutboxWriter outboxWriter, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new ArgumentException("UserId cannot be empty.", nameof(request.UserId));
        if (string.IsNullOrWhiteSpace(request.Symbol))
            throw new ArgumentException("Symbol cannot be empty.", nameof(request.Symbol));
        if (request.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(request.Quantity));
        if (request.Price <= 0)
            throw new ArgumentException("Price must be greater than zero.", nameof(request.Price));

        var order = Order.Create(request.UserId, request.Symbol, request.Quantity, request.Price);
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _orderRepository.AddAsync(order, cancellationToken);
            await _outboxWriter.EnqueueAsync(new OrderPlacedEvent(
                order.Id,
                order.UserId,
                order.Symbol,
                order.Quantity,
                order.Price
            ), cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        return order.Id;
    }
}
