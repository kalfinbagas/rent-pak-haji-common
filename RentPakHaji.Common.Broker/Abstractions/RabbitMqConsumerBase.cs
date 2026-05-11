using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace RentPakHaji.Common.Broker.Abstractions;

/// <summary>
/// Base class for RabbitMQ consumers.
/// Each service creates a concrete consumer that:
///   1. Calls base(queueName, logger) in constructor
///   2. Overrides HandleAsync to process the deserialized message
///
/// Pattern: one consumer class per event type (like SERA AstraFMS).
/// </summary>
public abstract class RabbitMqConsumerBase<TMessage> : BackgroundService
    where TMessage : class
{
    private readonly string _queueName;
    private readonly ILogger _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    protected RabbitMqConsumerBase(string queueName, ILogger logger)
    {
        _queueName = queueName;
        _logger = logger;
    }

    protected abstract IConnectionFactory ConnectionFactory { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = await ConnectionFactory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            string? body = null;
            try
            {
                body = Encoding.UTF8.GetString(ea.Body.Span);
                var message = JsonSerializer.Deserialize<TMessage>(body);

                if (message is null)
                {
                    _logger.LogWarning("Received null message on queue {Queue}. Nacking.", _queueName);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    return;
                }

                await HandleAsync(message, stoppingToken);
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message on queue {Queue}. Body: {Body}", _queueName, body);
                // Requeue = false so dead-letter queue can catch it
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Consumer started on queue {Queue}", _queueName);

        // Keep alive until stopped
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    /// <summary>Process the deserialized message. Override in each service.</summary>
    protected abstract Task HandleAsync(TMessage message, CancellationToken cancellationToken);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (_channel is not null) await _channel.CloseAsync(cancellationToken);
        if (_connection is not null) await _connection.CloseAsync(cancellationToken);
    }
}
