using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace WarrenMQ.Contracts;

public interface IRabbitMQChannelFactory
{
    Task<IChannel> GetChannelAsync(CancellationToken cancellationToken);
}