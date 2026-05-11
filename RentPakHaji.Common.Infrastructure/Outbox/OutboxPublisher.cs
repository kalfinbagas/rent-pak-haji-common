using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentPakHaji.Common.Infrastructure.Persistence;

namespace RentPakHaji.Common.Infrastructure.Outbox;

/// <summary>
/// Background service (IHostedService) that polls outbox_message for PENDING rows
/// and publishes them via the broker. Implements Transactional Outbox pattern.
///
/// Usage: In each service's Program.cs:
///   services.AddHostedService&lt;OutboxPublisher&lt;TDbContext&gt;&gt;();
///
/// The concrete service injects its own TDbContext (inherits BaseDbContext).
/// </summary>
public class OutboxPublisher<TDbContext> : BackgroundService
    where TDbContext : BaseDbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxPublisher<TDbContext>> _logger;
    private static readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public OutboxPublisher(
        IServiceProvider serviceProvider,
        ILogger<OutboxPublisher<TDbContext>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxPublisher started for {DbContext}", typeof(TDbContext).Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingMessagesAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        // Fetch up to 50 PENDING messages ordered by creation time
        var messages = await dbContext.OutboxMessages
            .Where(m => m.Status == "PENDING" && m.RetryCount < 5)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        if (messages.Count == 0) return;

        _logger.LogDebug("Processing {Count} outbox messages", messages.Count);

        // NOTE: Inject IRabbitMqPublisher here in concrete implementations.
        // This base class intentionally leaves the publish call as a virtual hook.
        foreach (var message in messages)
        {
            try
            {
                await PublishAsync(scope.ServiceProvider, message, ct);

                message.Status = "PUBLISHED";
                message.PublishedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {Id}", message.Id);
                message.RetryCount++;
                message.ErrorMessage = ex.Message;

                if (message.RetryCount >= 5)
                    message.Status = "FAILED";
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Override in concrete service to inject IRabbitMqPublisher and route the message.
    /// </summary>
    protected virtual Task PublishAsync(
        IServiceProvider scopedProvider,
        OutboxMessage message,
        CancellationToken ct)
    {
        _logger.LogWarning(
            "OutboxPublisher.PublishAsync not overridden. Message {Id} ({EventType}) was not sent.",
            message.Id, message.EventType);

        return Task.CompletedTask;
    }
}
