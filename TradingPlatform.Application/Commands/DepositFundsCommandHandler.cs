using MediatR;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Enums;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Commands;

public class DepositFundsCommandHandler : IRequestHandler<DepositFundsCommand, Unit>
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ITransactionRepository _transactionRepository;

    public DepositFundsCommandHandler(
        IPortfolioRepository portfolioRepository,
        ITransactionRepository transactionRepository)
    {
        _portfolioRepository = portfolioRepository;
        _transactionRepository = transactionRepository;
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

        var transaction = Transaction.Create(null, TransactionType.Deposit, request.Amount);
        await _transactionRepository.AddAsync(transaction, cancellationToken);

        return Unit.Value;
    }
}
