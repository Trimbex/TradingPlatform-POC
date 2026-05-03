using MediatR;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Enums;
using TradingPlatform.Domain.Events;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Commands;

public class DepositFundsCommandHandler : IRequestHandler<DepositFundsCommand, Unit>
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;

    public DepositFundsCommandHandler(
        IPortfolioRepository portfolioRepository,
        ITransactionRepository transactionRepository,
        IOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork)
    {
        _portfolioRepository = portfolioRepository;
        _transactionRepository = transactionRepository;
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var portfolio = await _portfolioRepository.GetByUserAsync(request.UserId, cancellationToken);

            if (portfolio is null)
            {
                portfolio = Portfolio.Create(request.UserId);
                await _portfolioRepository.AddAsync(portfolio, cancellationToken);
            }

            portfolio.AddFunds(request.Amount);
            await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);

            var transaction = Transaction.Create(request.UserId, null, TransactionType.Deposit, request.Amount);
            await _transactionRepository.AddAsync(transaction, cancellationToken);

            await _outboxWriter.EnqueueAsync(new FundsDepositedEvent(request.UserId, request.Amount), cancellationToken);
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
