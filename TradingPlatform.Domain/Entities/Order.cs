using TradingPlatform.Domain.Enums;

namespace TradingPlatform.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string Symbol { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal Price { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Order() { }

    public static Order Create(string userId, string symbol, decimal quantity, decimal price)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero.", nameof(price));

        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Symbol = symbol.Trim(),
            Quantity = quantity,
            Price = price,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Execute()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot execute order in {Status} status. Only Pending orders can be executed.");

        Status = OrderStatus.Executed;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot cancel order in {Status} status. Only Pending orders can be cancelled.");

        Status = OrderStatus.Cancelled;
    }
}
