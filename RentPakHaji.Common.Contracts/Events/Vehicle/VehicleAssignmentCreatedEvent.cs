namespace RentPakHaji.Common.Contracts.Events.Vehicle;

/// <summary>
/// Published by Vehicle service when a VehicleAssignment is created
/// (triggered by receiving PaymentSuccessEvent).
///
/// Consumers:
///   - BookingOrder service: replicate assignment data
///   - Journey service: create initial journey record
///   - Driver service: link driver to assignment
///   - Notification service: notify customer of vehicle assigned
///
/// Exchange : rpk.vehicle
/// Routing  : vehicle.assignment.created
/// </summary>
public sealed record VehicleAssignmentCreatedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public Guid TransactionId { get; init; }
    public Guid AssignmentId { get; init; }
    public Guid BookingOrderId { get; init; }
    public int Sequence { get; init; }

    // Vehicle denorm
    public Guid VehicleId { get; init; }
    public string RegistrationNumber { get; init; } = string.Empty;
    public string VehicleCategory { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;

    // Pool denorm
    public Guid PoolLocationId { get; init; }
    public string PoolLocationName { get; init; } = string.Empty;

    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
