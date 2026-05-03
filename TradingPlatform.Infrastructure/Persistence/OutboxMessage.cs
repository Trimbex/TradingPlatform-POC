namespace TradingPlatform.Infrastructure.Persistence;

public class OutboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public DateTime OccurredOnUtc { get; init; }
    public DateTime? ProcessedOnUtc { get; set; }
    /// <summary>When the dispatcher should try this row again (UTC). Ignored once processed.</summary>
    public DateTime NextAttemptAtUtc { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
    /// <summary>Set when max retries exceeded; row is no longer dispatched.</summary>
    public DateTime? DeadLetteredAtUtc { get; set; }
}
