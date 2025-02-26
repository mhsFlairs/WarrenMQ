using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using WarrenMQ.Configuration;
using WarrenMQ.Contracts;

namespace WarrenMQ.Factories;

/// <summary>
/// Provides a factory for creating and managing RabbitMQ connections.
/// Ensures that only one active connection exists at any given time,
/// handling connection creation, disposal, and reuse in a thread-safe manner.
/// </summary>
public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory
{
    private readonly RabbitMQConfig _config;
    private readonly ILogger<RabbitMQConnectionFactory> _logger;
    private IConnection? _connection;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public RabbitMQConnectionFactory(
        IOptions<RabbitMQConfig> config,
        ILogger<RabbitMQConnectionFactory> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public RabbitMQConnectionFactory(
        RabbitMQConfig config,
        ILogger<RabbitMQConnectionFactory> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously retrieves an active connection. If an existing connection is open, it returns that connection.
    /// Otherwise, it establishes a new connection using the configured settings. This method ensures that only one
    /// connection is created at a time by utilizing a semaphore for synchronization. If an existing connection
    /// is present but not open, it attempts to dispose of it before creating a new one.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the active <see cref="IConnection"/>.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    /// <exception cref="Exception">Propagates any exception thrown during the connection disposal or creation process.</exception>
    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            if (_connection != null)
            {
                try
                {
                    await _connection.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing RabbitMQ connection");
                }
            }

            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = _config.HostName,
                UserName = _config.UserName,
                Password = _config.Password,
                Port = _config.Port,
                VirtualHost = _config.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);

            return _connection;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}