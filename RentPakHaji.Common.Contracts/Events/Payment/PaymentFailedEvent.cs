namespace RentPakHaji.Common.Contracts.Events.Payment;

/// <summary>
/// Published by Payment service when a payment attempt fails (gateway rejection,
/// VA expired, QRIS timeout, insufficient balance, etc.)
///
/// Consumers:
///   - BookingOrder service: increment retry count or transition to EXPIRED
///   - Notification service: send "Payment failed, please retry" push notification
///
/// Exchange : rpk.payment
/// Routing  : payment.failed
/// </summary>
public sealed record PaymentFailedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public Guid TransactionId { get; init; }
    public Guid PaymentId { get; init; }
    public Guid InvoiceId { get; init; }
    public Guid BookingOrderId { get; init; }

    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;

    /// <summary>e.g. "VA_EXPIRED", "INSUFFICIENT_BALANCE", "GATEWAY_TIMEOUT"</summary>
    public string FailureCode { get; init; } = string.Empty;
    public string FailureMessage { get; init; } = string.Empty;

    public DateTime FailedAt { get; init; }
}
