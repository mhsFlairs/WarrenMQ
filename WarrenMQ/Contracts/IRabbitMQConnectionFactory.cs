using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace WarrenMQ.Contracts;

public interface IRabbitMQConnectionFactory
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken);
}