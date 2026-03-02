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
    private readonly IEventPublisher _eventPublisher;

    public DepositFundsCommandHandler(
        IPortfolioRepository portfolioRepository,
        ITransactionRepository transactionRepository,
        IEventPublisher eventPublisher)
    {
        _portfolioRepository = portfolioRepository;
        _transactionRepository = transactionRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Unit> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
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

        await _eventPublisher.PublishAsync(new FundsDepositedEvent(request.UserId, request.Amount), cancellationToken);

        return Unit.Value;
    }
}
