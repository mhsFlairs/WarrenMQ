namespace WarrenMQ.Contracts;

public interface IRabbitMQService
{
    Task PublishFanOutMessageAsync<T>(
        T message,
        string exchangeName,
        CancellationToken cancellationToken);

    Task ConsumeFanOutMessagesAsync<T>
    (string queueName,
        string exchangeName,
        Func<T, Task> messageHandler,
        CancellationToken cancellationToken);
}