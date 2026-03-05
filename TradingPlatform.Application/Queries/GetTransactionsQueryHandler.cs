using MediatR;
using TradingPlatform.Application.DTOs;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Queries;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, IReadOnlyList<TransactionDto>>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionsQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<IReadOnlyList<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByUserAsync(request.UserId, cancellationToken);

        return transactions
            .Select(MapToDto)
            .ToList();
    }

    private static TransactionDto MapToDto(Transaction transaction) =>
        new(
            transaction.Id,
            transaction.UserId,
            transaction.OrderId,
            transaction.Type.ToString(),
            transaction.Amount,
            transaction.Timestamp,
            transaction.Status.ToString()
        );
}

