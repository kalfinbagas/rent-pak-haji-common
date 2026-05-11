namespace RentPakHaji.Common.Contracts.Events.Booking;

/// <summary>
/// Published by BookingOrder service when payment window expires.
/// The Coravel scheduler runs every minute querying v_expiring_orders,
/// transitions order status to EXPIRED, then publishes this event.
///
/// Consumers:
///   - Vehicle service: release soft booking
///   - Payment service: mark invoice as CANCELLED (if any)
///   - Notification service: send "Booking expired" push notification
///
/// Exchange : rpk.booking
/// Routing  : booking.order.expired
/// </summary>
public sealed record BookingOrderExpiredEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public Guid TransactionId { get; init; }
    public Guid BookingOrderId { get; init; }
    public string BookingCode { get; init; } = string.Empty;

    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;

    public DateTime PaymentExpiresAt { get; init; }
    public DateTime ExpiredAt { get; init; }
}
