using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WarrenMQ.Contracts;

namespace WarrenMQ.Services;

/// <summary>
/// Service for publishing and consuming messages using RabbitMQ's fanout exchanges.
/// <para>
/// This service provides methods to publish messages to a specified fanout exchange and to consume messages
/// from a fanout exchange by setting up a dedicated queue with a unique identifier. It leverages an
/// <see cref="IRabbitMQChannelFactory"/> to manage RabbitMQ channels and utilizes logging through
/// <see cref="ILogger{RabbitMQService}"/> to track operations and handle errors.
/// </para>
/// 
/// <para>
/// The <see cref="PublishFanOutMessageAsync{T}"/> method serializes and sends messages to the designated
/// exchange, ensuring that all bound queues receive the published messages. The <see cref="ConsumeFanOutMessagesAsync{T}"/>
/// method sets up a consumer that listens to messages from the specified exchange, deserializes incoming
/// messages, and processes them using the provided handler function.
/// </para>
/// </summary>
public class RabbitMQService : IRabbitMQService
{
    private readonly IRabbitMQChannelFactory _channelFactory;
    private readonly ILogger<RabbitMQService> _logger;

    public RabbitMQService(
        IRabbitMQChannelFactory channelFactory,
        ILogger<RabbitMQService> logger)
    {
        _channelFactory = channelFactory;
        _logger = logger;
    }

    /// <summary>
    /// Publishes a message to a fanout exchange in a message broker.
    /// </summary>
    /// <typeparam name="T">The type of the message being published.</typeparam>
    /// <param name="message">The message to be published.</param>
    /// <param name="exchangeName">The name of the fanout exchange where the message will be published.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="Exception">Throws an exception if there is an error during publishing.</exception>
    public async Task PublishFanOutMessageAsync<T>(
        T message,
        string exchangeName,
        CancellationToken cancellationToken)
    {
        try
        {
            IChannel channel = await _channelFactory.GetChannelAsync(cancellationToken: cancellationToken);
            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, cancellationToken: cancellationToken);
            string serializedMessage = JsonSerializer.Serialize(message);
            byte[] body = Encoding.UTF8.GetBytes(serializedMessage);

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: "",
                body: body,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message");
            throw;
        }
    }

    /// <summary>
    /// Consumes messages from a fan-out exchange in a message queue.
    /// </summary>
    /// <typeparam name="T">The type of the message to be consumed.</typeparam>
    /// <param name="queueNamePrefix">The prefix for the queue name, which will be suffixed with a unique identifier.</param>
    /// <param name="exchangeName">The name of the exchange to bind the queue to.</param>
    /// <param name="messageHandler">A function that processes the received message asynchronously.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="Exception">Thrown if an error occurs while setting up the consumer or processing messages.</exception>
    public async Task ConsumeFanOutMessagesAsync<T>(
        string queueNamePrefix,
        string exchangeName,
        Func<T, Task> messageHandler,
        CancellationToken cancellationToken)

    {
        try
        {
            IChannel channel = await _channelFactory.GetChannelAsync(cancellationToken: cancellationToken);

            string queueName = $"{queueNamePrefix}-{Guid.NewGuid()}";

            await channel.QueueDeclareAsync(queueName, durable: false, exclusive: false, autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(queueName, exchangeName, "", cancellationToken: cancellationToken);

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    byte[] body = ea.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    T? deserializedMessage = JsonSerializer.Deserialize<T>(message);

                    if (deserializedMessage != null)
                    {
                        await messageHandler(deserializedMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                }
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up consumer");
            throw;
        }
    }
}