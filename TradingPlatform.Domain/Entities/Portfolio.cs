namespace TradingPlatform.Domain.Entities;

public class Portfolio
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public decimal CashBalance { get; private set; }
    private readonly List<Holding> _holdings = [];
    public IReadOnlyList<Holding> Holdings => _holdings.AsReadOnly();

    private Portfolio() { }

    public static Portfolio Create(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        return new Portfolio
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CashBalance = 0
        };
    }

    public void AddFunds(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        CashBalance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        if (amount > CashBalance)
            throw new InvalidOperationException($"Insufficient funds. Balance: {CashBalance}, Requested: {amount}.");

        CashBalance -= amount;
    }

    public void AddHolding(string symbol, decimal quantity, decimal price)
    {
        var existing = _holdings.Find(h => h.Symbol.Equals(symbol.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
            existing.AddQuantity(quantity, price);
        else
            _holdings.Add(Holding.Create(symbol, quantity, price));
    }

    public void RemoveHolding(string symbol, decimal quantity)
    {
        var existing = _holdings.Find(h => h.Symbol.Equals(symbol.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No holding found for symbol '{symbol}'.");

        existing.RemoveQuantity(quantity);

        if (existing.Quantity == 0)
            _holdings.Remove(existing);
    }
}
