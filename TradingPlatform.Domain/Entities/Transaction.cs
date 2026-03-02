using TradingPlatform.Domain.Enums;

namespace TradingPlatform.Domain.Entities;

public class Transaction
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public Guid? OrderId { get; init; }
    public TransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public DateTime Timestamp { get; init; }
    public TransactionStatus Status { get; init; }

    private Transaction() { }

    public static Transaction Create(string userId, Guid? orderId, TransactionType type, decimal amount, TransactionStatus status = TransactionStatus.Completed)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        return new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            OrderId = orderId,
            Type = type,
            Amount = amount,
            Timestamp = DateTime.UtcNow,
            Status = status
        };
    }
}
