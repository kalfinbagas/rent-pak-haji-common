namespace RentPakHaji.Common.Contracts.Events.Booking;

/// <summary>
/// Published by BookingOrder service when a VehicleSoftBooking is created.
/// Consumed by Vehicle service to replicate the soft-booking record into rpk_vehicle
/// so the inventory service can calculate available stock without cross-DB joins.
///
/// Exchange : rpk.booking
/// Routing  : booking.soft-booking.created
/// </summary>
public sealed record SoftBookingCreatedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    // Correlation
    public Guid TransactionId { get; init; }
    public Guid BookingOrderId { get; init; }
    public int Sequence { get; init; }

    // Soft booking data
    public Guid SoftBookingId { get; init; }
    public Guid VehicleId { get; init; }
    public string VehicleRegistrationNumber { get; init; } = string.Empty;
    public string VehicleCategoryName { get; init; } = string.Empty;

    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    public int NumberOfVehicles { get; init; }

    // Pool denorm
    public Guid PoolLocationId { get; init; }
    public string PoolLocationName { get; init; } = string.Empty;

    public DateTime ExpiredAt { get; init; }
}
