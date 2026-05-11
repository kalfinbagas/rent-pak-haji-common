using Microsoft.EntityFrameworkCore;
using RentPakHaji.Common.Domain.Primitives;
using RentPakHaji.Common.Domain.Repositories;
using RentPakHaji.Common.Infrastructure.Outbox;
using System.Text.Json;

namespace RentPakHaji.Common.Infrastructure.Persistence;

/// <summary>
/// Base EF Core DbContext for all RPK services.
/// Responsibilities:
///   1. Auto-set audit fields (CreatedAt, UpdatedAt, Version) on SaveChanges
///   2. Dispatch domain events to outbox_message table (Transactional Outbox pattern)
/// Each service inherits this and adds its own DbSets + Configurations.
/// </summary>
public abstract class BaseDbContext : DbContext, IUnitOfWork
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected BaseDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("outbox_message");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.EventType).HasMaxLength(200).IsRequired();
            builder.Property(o => o.Payload).IsRequired();
            builder.Property(o => o.Status).HasMaxLength(20).IsRequired();
            builder.HasIndex(o => new { o.Status, o.CreatedAt })
                   .HasFilter("status = 'PENDING'");
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        await DispatchDomainEventsToOutboxAsync();
        return await base.SaveChangesAsync(cancellationToken);
    }

    // ─── Private helpers ──────────────────────────────────────────

    private void ApplyAuditFields()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.IsActive = true;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.Version++;
                    break;
            }
        }
    }

    private Task DispatchDomainEventsToOutboxAsync()
    {
        var entitiesWithEvents = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                OutboxMessages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    EventType = domainEvent.GetType().FullName!,
                    Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    CreatedAt = DateTime.UtcNow,
                    Status = "PENDING"
                });
            }

            entity.ClearDomainEvents();
        }

        return Task.CompletedTask;
    }
}
