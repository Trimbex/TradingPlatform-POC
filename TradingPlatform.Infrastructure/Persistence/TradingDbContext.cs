using Microsoft.EntityFrameworkCore;
using TradingPlatform.Domain.Entities;

namespace TradingPlatform.Infrastructure.Persistence;

public class TradingDbContext : DbContext
{
    public TradingDbContext(DbContextOptions<TradingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.Price).HasPrecision(18, 4);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Portfolio
        modelBuilder.Entity<Portfolio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CashBalance).HasPrecision(18, 4);

            entity.OwnsMany(p => p.Holdings, holding =>
            {
                holding.WithOwner().HasForeignKey("PortfolioId");
                holding.Property(h => h.Symbol).HasMaxLength(10);
                holding.Property(h => h.Quantity).HasPrecision(18, 4);
                holding.Property(h => h.AveragePrice).HasPrecision(18, 4);
                holding.HasKey("PortfolioId", "Symbol");
            });
            entity.Navigation(e => e.Holdings).HasField("_holdings").AutoInclude();
        });

        // Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId);
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 4);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
