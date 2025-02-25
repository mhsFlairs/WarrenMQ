using Microsoft.Extensions.Logging;
using WarrenMQ.Configuration;
using WarrenMQ.Contracts;
using WarrenMQ.Factories;
using WarrenMQ.Services;

const string exchangeName = "playground-exchange";

RabbitMQConfig config = new RabbitMQConfig
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",
    Port = 5672,
    VirtualHost = "/",
    ChannelPoolSize = 10
};

if (args.Length != 1)
{
    Console.WriteLine("Usage: RabbitMQApp <publisher/consumer>");
    return;
}

string mode = args[0].ToLower();

if (mode == "publisher")
{
    await PublishMessageAsync();
}
else if (mode == "consumer")
{
    await ConsumerMessageFromPackageAsync();
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

    Console.WriteLine("Press any key to publish a message...");
    Console.ReadKey();

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

async Task ConsumerMessageFromPackageAsync()
{
    IRabbitMQConnectionFactory connectionFactory =
        new RabbitMQConnectionFactory(config, new ConsoleLogger<RabbitMQConnectionFactory>());

    IRabbitMQChannelFactory channelFactory =
        new RabbitMQChannelFactory(connectionFactory, config, new ConsoleLogger<RabbitMQChannelFactory>());

    IRabbitMQService rabbitMQService = new RabbitMQService(channelFactory, new ConsoleLogger<RabbitMQService>());

    string queueName = $"Playground-{Guid.NewGuid()}";

    Console.WriteLine($"Queue name: {queueName}. Waiting for messages...");

    await rabbitMQService.ConsumeFanOutMessagesAsync<string>(queueName, exchangeName, message =>
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