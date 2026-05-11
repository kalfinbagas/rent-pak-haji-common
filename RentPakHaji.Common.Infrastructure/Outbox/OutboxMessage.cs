namespace RentPakHaji.Common.Infrastructure.Outbox;

/// <summary>
/// Outbox message entity — persisted in the same transaction as the business change.
/// A background publisher (OutboxPublisher) picks up PENDING messages and sends to RabbitMQ.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;

    /// <summary>PENDING | PUBLISHED | FAILED</summary>
    public string Status { get; set; } = "PENDING";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>Number of publish attempts (for retry logic).</summary>
    public int RetryCount { get; set; }
}
