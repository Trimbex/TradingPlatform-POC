using MediatR;
using TradingPlatform.Application.Exceptions;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Enums;
using TradingPlatform.Domain.Events;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Commands;

public class ExecuteOrderCommandHandler : IRequestHandler<ExecuteOrderCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;

    public ExecuteOrderCommandHandler(
        IOrderRepository orderRepository,
        IPortfolioRepository portfolioRepository,
        ITransactionRepository transactionRepository,
        IOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _portfolioRepository = portfolioRepository;
        _transactionRepository = transactionRepository;
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ExecuteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Order {request.OrderId} not found.");

        var portfolio = await _portfolioRepository.GetByUserAsync(order.UserId, cancellationToken)
            ?? throw new NotFoundException($"Portfolio for user {order.UserId} not found.");

        var orderTotal = order.Quantity * order.Price;

        order.Execute();
        portfolio.Withdraw(orderTotal);
        portfolio.AddHolding(order.Symbol, order.Quantity, order.Price);

        var transaction = Transaction.Create(order.UserId, order.Id, TransactionType.OrderPayment, orderTotal);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);
            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _outboxWriter.EnqueueAsync(new OrderExecutedEvent(order.Id), cancellationToken);
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
