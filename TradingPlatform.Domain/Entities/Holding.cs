namespace TradingPlatform.Domain.Entities;

public class Holding
{
    public string Symbol { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal AveragePrice { get; private set; }

    private Holding() { }

    public static Holding Create(string symbol, decimal quantity, decimal price)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero.", nameof(price));

        return new Holding
        {
            Symbol = symbol.Trim(),
            Quantity = quantity,
            AveragePrice = price
        };
    }

    public void AddQuantity(decimal quantity, decimal price)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero.", nameof(price));

        var totalCost = (Quantity * AveragePrice) + (quantity * price);
        Quantity += quantity;
        AveragePrice = totalCost / Quantity;
    }

    public void RemoveQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (quantity > Quantity)
            throw new InvalidOperationException($"Cannot remove {quantity} shares. Only {Quantity} shares held.");

        Quantity -= quantity;
    }

    public bool HasQuantity(decimal quantity) => Quantity >= quantity;
}
