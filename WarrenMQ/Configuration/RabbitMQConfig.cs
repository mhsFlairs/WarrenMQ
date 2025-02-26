namespace WarrenMQ.Configuration;

/// <summary>
/// Configuration class for RabbitMQ connection settings.
/// </summary>
public
    class RabbitMQConfig
{
    /// <summary>
    /// The configuration section name used in configuration files.
    /// </summary>
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// Gets the hostname of the RabbitMQ server.
    /// Default value is "localhost".
    /// </summary>
    public string HostName { get; init; } = "localhost";

    /// <summary>
    /// Gets the username for authentication with the RabbitMQ server.
    /// Default value is "guest".
    /// </summary>
    public string UserName { get; init; } = "guest";

    /// <summary>
    /// Gets the password for authentication with the RabbitMQ server.
    /// Default value is "guest".
    /// </summary>
    public string Password { get; init; } = "guest";

    /// <summary>
    /// Gets the port number for the RabbitMQ server connection.
    /// Default value is 5672.
    /// </summary>
    public int Port { get; init; } = 5672;

    /// <summary>
    /// Gets the virtual host name for the RabbitMQ connection.
    /// Default value is "/".
    /// </summary>
    public string VirtualHost { get; init; } = "/";

    /// <summary>
    /// Gets the size of the channel pool for managing RabbitMQ channels.
    /// Default value is 10.
    /// </summary>
    public int ChannelPoolSize { get; init; } = 10;

    /// <summary>
    /// Initializes a new instance of the RabbitMQConfig class with default values.
    /// </summary>
    public RabbitMQConfig()
    {
    }

    /// <summary>
    /// Initializes a new instance of the RabbitMQConfig class with specified values.
    /// </summary>
    /// <param name="hostName">The hostname of the RabbitMQ server.</param>
    /// <param name="userName">The username for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    /// <param name="port">The port number for the connection.</param>
    /// <param name="virtualHost">The virtual host name.</param>
    /// <param name="channelPoolSize">The size of the channel pool.</param>
    public RabbitMQConfig(
        string hostName,
        string userName,
        string password,
        int port,
        string virtualHost,
        int channelPoolSize)
    {
        HostName = hostName;
        UserName = userName;
        Password = password;
        Port = port;
        VirtualHost = virtualHost;
        ChannelPoolSize = channelPoolSize;
    }
}