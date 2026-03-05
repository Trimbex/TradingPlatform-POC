using MediatR;
using TradingPlatform.Application.DTOs;

namespace TradingPlatform.Application.Queries;

public record GetTransactionsQuery(string UserId) : IRequest<IReadOnlyList<TransactionDto>>;

