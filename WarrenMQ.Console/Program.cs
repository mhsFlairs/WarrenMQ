using Microsoft.Extensions.Logging;
using WarrenMQ.Configuration;
using WarrenMQ.Contracts;
using WarrenMQ.Factories;
using WarrenMQ.Services;

const string exchangeName = "IntegrationOutputQueue";

string queueName = "Playground";

RabbitMQConfig config = new RabbitMQConfig
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",
    Port = 5672,
    VirtualHost = "/",
    ChannelPoolSize = 10
};

if (args.Length > 2 || args.Length == 0)
{
    Console.WriteLine("Usage: RabbitMQApp <publisher/consumer> <optional:queueName>");
    return;
}

string mode = args[0].ToLower();

queueName = string.IsNullOrWhiteSpace(args.ElementAtOrDefault(1)) ? queueName : args[1];

if (mode == "publisher")
{
    await PublishMessageAsync();
}
else if (mode == "consumer")
{
    await ConsumerMessageFromPackageAsync(queueName);
}
else
{
    Console.WriteLine("Invalid mode. Please use 'publisher' or 'consumer'.");
}

async Task PublishMessageAsync()
{
    IRabbitMQConnectionFactory connectionFactory =
        new RabbitMQConnectionFactory(config, new ConsoleLogger<RabbitMQConnectionFactory>());

    IRabbitMQChannelFactory channelFactory =
        new RabbitMQChannelFactory(connectionFactory, config, new ConsoleLogger<RabbitMQChannelFactory>());

    IRabbitMQService rabbitMQService = new RabbitMQService(channelFactory, new ConsoleLogger<RabbitMQService>());

    while (true)
    {
        Console.WriteLine("Enter message to publish (or 'e' to exit):");
        string? message = Console.ReadLine();

        if (message?.ToLower() == "e")
        {
            break;
        }

        await rabbitMQService.PublishFanOutMessageAsync(message, exchangeName, CancellationToken.None);

        Console.WriteLine("Message published successfully.");
    }
}

async Task ConsumerMessageFromPackageAsync(string _queueName)
{
    IRabbitMQConnectionFactory connectionFactory =
        new RabbitMQConnectionFactory(config, new ConsoleLogger<RabbitMQConnectionFactory>());

    IRabbitMQChannelFactory channelFactory =
        new RabbitMQChannelFactory(connectionFactory, config, new ConsoleLogger<RabbitMQChannelFactory>());

    IRabbitMQService rabbitMQService = new RabbitMQService(channelFactory, new ConsoleLogger<RabbitMQService>());

    Console.WriteLine($"Queue name: {_queueName}. Waiting for messages...");

    await rabbitMQService.ConsumeFanOutMessagesAsync<object>(_queueName, exchangeName, message =>
    {
        Console.WriteLine($"Received message:{message}");
        return Task.CompletedTask;
    }, CancellationToken.None);

    Console.WriteLine("Press any key to exit...");
    Console.ReadLine();
}

class ConsoleLogger<T> : ILogger<T>
{
    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        if (exception != null)
        {
            Console.WriteLine(formatter(state, exception));
        }
    }
}