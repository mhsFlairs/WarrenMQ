using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using WarrenMQ.Configuration;
using WarrenMQ.Contracts;

namespace WarrenMQ.Factories;

/// <summary>
/// A thread-safe factory for managing RabbitMQ channels in a pool-based system.
/// This factory implements channel pooling with thread affinity, where each thread
/// maintains its own dedicated channel. It manages channel lifecycle, handles channel
/// errors and shutdowns, and provides automatic channel recovery.
/// The factory ensures efficient channel reuse while maintaining thread safety through
/// semaphore-based synchronization.
/// </summary>
public class RabbitMQChannelFactory : IRabbitMQChannelFactory
{
    private readonly IRabbitMQConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMQChannelFactory> _logger;
    private readonly ConcurrentDictionary<int, IChannel> _channels = new();
    private readonly int _maxPoolSize;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    RabbitMQChannelFactory(
        IRabbitMQConnectionFactory connectionFactory,
        int maxPoolSize,
        ILogger<RabbitMQChannelFactory> logger)
    {
        _connectionFactory = connectionFactory;
        _maxPoolSize = maxPoolSize > 0 ? maxPoolSize : 1;
        _logger = logger;
    }

    public RabbitMQChannelFactory(
        IRabbitMQConnectionFactory connectionFactory,
        IOptions<RabbitMQConfig> config,
        ILogger<RabbitMQChannelFactory> logger) : this(connectionFactory, config.Value.ChannelPoolSize, logger)
    {
    }

    public RabbitMQChannelFactory(
        IRabbitMQConnectionFactory connectionFactory,
        RabbitMQConfig config,
        ILogger<RabbitMQChannelFactory> logger) : this(connectionFactory, config.ChannelPoolSize, logger)
    {
    }

    /// <summary>
    /// Asynchronously retrieves an open channel associated with the current thread. 
    /// Utilizes a thread-specific pool to reuse existing channels when available,
    /// or creates and adds a new channel to the pool if necessary. 
    /// Ensures thread safety and limits the maximum number of concurrent channels
    /// using a semaphore to control access to the channel pool.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the <see cref="IChannel"/> instance associated with the current thread.
    /// </returns>
    public async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        int threadId = Thread.CurrentThread.ManagedThreadId % _maxPoolSize;

        if (_channels.TryGetValue(threadId, out IChannel? existingChannel) && existingChannel.IsOpen)
        {
            return existingChannel;
        }

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_channels.TryGetValue(threadId, out existingChannel) && existingChannel.IsOpen)
            {
                return existingChannel;
            }

            IChannel channel = await CreateNewChannelAsync(cancellationToken);

            _channels.AddOrUpdate(threadId, channel, (_, _) => channel);

            return channel;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private async Task<IChannel> CreateNewChannelAsync(CancellationToken cancellationToken)
    {
        IConnection connection = await _connectionFactory.GetConnectionAsync(cancellationToken);

        IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        channel.CallbackExceptionAsync += async (sender, args) =>
        {
            _logger.LogError(args.Exception, "Channel callback exception. {0}", args.Exception.Message);
            await HandleChannelErrorAsync(sender as IChannel);
        };

        channel.ChannelShutdownAsync += async (sender, args) =>
        {
            _logger.LogWarning("Channel shutdown: {0}", args.ReplyText);
            await HandleChannelErrorAsync(sender as IChannel);
        };

        return channel;
    }
    
    private async Task HandleChannelErrorAsync(IChannel? channel)
    {
        try
        {
            int threadId = Thread.CurrentThread.ManagedThreadId % _maxPoolSize;

            if (_channels.TryRemove(threadId, out _))
            {
                if (channel is { IsOpen: true })
                {
                    await channel.CloseAsync();
                }

                channel?.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling channel error");
            throw;
        }
    }
}