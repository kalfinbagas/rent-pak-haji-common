namespace RentPakHaji.Common.Contracts.Events.Payment;

/// <summary>
/// Published by Payment service when a payment is confirmed (gateway callback).
///
/// Consumers:
///   - BookingOrder service: transition order to CONFIRMED, release soft booking
///   - Vehicle service: convert soft booking → VehicleAssignment
///   - Notification service: send payment receipt
///
/// Exchange : rpk.payment
/// Routing  : payment.success
/// </summary>
public sealed record PaymentSuccessEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public Guid TransactionId { get; init; }
    public Guid PaymentId { get; init; }
    public Guid InvoiceId { get; init; }
    public Guid BookingOrderId { get; init; }

    public decimal AmountPaid { get; init; }
    public string Currency { get; init; } = "IDR";
    public string PaymentMethod { get; init; } = string.Empty;

    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;

    public DateTime PaidAt { get; init; }
}
