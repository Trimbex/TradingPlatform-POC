using MediatR;
using TradingPlatform.Application.Exceptions;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Enums;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Commands;

public class WithdrawFundsCommandHandler : IRequestHandler<WithdrawFundsCommand, Unit>
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ITransactionRepository _transactionRepository;

    public WithdrawFundsCommandHandler(
        IPortfolioRepository portfolioRepository,
        ITransactionRepository transactionRepository)
    {
        _portfolioRepository = portfolioRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Unit> Handle(WithdrawFundsCommand request, CancellationToken cancellationToken)
    {
        var portfolio = await _portfolioRepository.GetByUserAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"Portfolio for user {request.UserId} not found.");

        portfolio.Withdraw(request.Amount);
        await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);

        var transaction = Transaction.Create(null, TransactionType.Withdrawal, request.Amount);
        await _transactionRepository.AddAsync(transaction, cancellationToken);

        return Unit.Value;
    }
}
