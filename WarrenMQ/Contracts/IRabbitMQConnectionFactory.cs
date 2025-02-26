using RabbitMQ.Client;

namespace WarrenMQ.Contracts;

public interface IRabbitMQConnectionFactory
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken);
}