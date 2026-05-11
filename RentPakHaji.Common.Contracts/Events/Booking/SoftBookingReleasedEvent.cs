namespace RentPakHaji.Common.Contracts.Events.Booking;

/// <summary>
/// Published when a soft booking is released — either because:
///   - Payment succeeded (booking proceeds to CONFIRMED)
///   - Payment timed out / order expired
///   - Order was cancelled
///
/// Consumed by Vehicle service to update the replicated record status to RELEASED.
///
/// Exchange : rpk.booking
/// Routing  : booking.soft-booking.released
/// </summary>
public sealed record SoftBookingReleasedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public Guid TransactionId { get; init; }
    public Guid SoftBookingId { get; init; }
    public Guid BookingOrderId { get; init; }

    /// <summary>PAYMENT_SUCCESS | EXPIRED | CANCELLED</summary>
    public string Reason { get; init; } = string.Empty;
}
