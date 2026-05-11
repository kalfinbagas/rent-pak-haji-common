using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RentPakHaji.Common.Broker.Abstractions;
using System.Text;
using System.Text.Json;

namespace RentPakHaji.Common.Broker.Publisher;

/// <summary>
/// Concrete RabbitMQ publisher.
/// Register as singleton in each service's DI container.
/// </summary>
public sealed class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConnection connection, ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task PublishAsync<T>(
        T message,
        string exchange,
        string routingKey,
        CancellationToken cancellationToken = default)
        where T : class
    {
        await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Published {EventType} to exchange {Exchange} with key {RoutingKey}",
            typeof(T).Name, exchange, routingKey);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.CloseAsync();
        _connection.Dispose();
    }
}
