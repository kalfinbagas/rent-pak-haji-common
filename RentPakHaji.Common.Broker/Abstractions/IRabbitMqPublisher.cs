namespace RentPakHaji.Common.Broker.Abstractions;

/// <summary>
/// Abstraction for publishing messages to RabbitMQ.
/// Each service injects this to send integration events.
/// </summary>
public interface IRabbitMqPublisher
{
    /// <summary>
    /// Publish a message to the specified exchange with a routing key.
    /// </summary>
    Task PublishAsync<T>(
        T message,
        string exchange,
        string routingKey,
        CancellationToken cancellationToken = default)
        where T : class;
}
