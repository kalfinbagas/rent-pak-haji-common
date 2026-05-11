namespace RentPakHaji.Common.Domain.Primitives;

/// <summary>
/// Entity with full audit trail: created, updated, soft-delete, optimistic concurrency.
/// Pattern from SERA AstraFMS 2.0.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    /// <summary>Soft-delete flag. Queries should filter WHERE is_active = true.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Optimistic concurrency token — increment on every UPDATE.</summary>
    public int Version { get; set; } = 1;

    public void MarkUpdated(string? updatedBy = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsActive = false;
        MarkUpdated(deletedBy);
    }
}
