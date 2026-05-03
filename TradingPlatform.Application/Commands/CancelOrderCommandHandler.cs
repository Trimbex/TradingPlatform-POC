using MediatR;
using TradingPlatform.Application.Exceptions;
using TradingPlatform.Domain.Events;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Commands;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(IOrderRepository orderRepository, IOutboxWriter outboxWriter, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Order {request.OrderId} not found.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            order.Cancel();
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _outboxWriter.EnqueueAsync(new OrderCancelledEvent(order.Id), cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        return Unit.Value;
    }
}
