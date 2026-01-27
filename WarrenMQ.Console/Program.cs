using Microsoft.Extensions.Logging;
using WarrenMQ.Configuration;
using WarrenMQ.Contracts;
using WarrenMQ.Factories;
using WarrenMQ.Services;

const string exchangeName = "integration_output_exchange";

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
    Console.WriteLine("Usage: RabbitMQApp <publisher/consumer/topic-publisher/topic-consumer> <optional:queueName>");
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
else if (mode == "topic-publisher")
{
    await PublishTopicMessageAsync();
}
else if (mode == "topic-consumer")
{
    await ConsumeTopicMessageAsync(queueName);
}
else
{
    Console.WriteLine("Invalid mode. Please use 'publisher', 'consumer', 'topic-publisher', or 'topic-consumer'.");
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

async Task PublishTopicMessageAsync()
{
    IRabbitMQConnectionFactory connectionFactory =
        new RabbitMQConnectionFactory(config, new ConsoleLogger<RabbitMQConnectionFactory>());

    IRabbitMQChannelFactory channelFactory =
        new RabbitMQChannelFactory(connectionFactory, config, new ConsoleLogger<RabbitMQChannelFactory>());

    IRabbitMQService rabbitMQService = new RabbitMQService(channelFactory, new ConsoleLogger<RabbitMQService>());

    Console.WriteLine("Topic Publisher Started!");
    Console.WriteLine($"Exchange: {exchangeName}");
    Console.WriteLine("Routing Key: pode2");

    while (true)
    {
        Console.WriteLine("\nEnter message to publish (or 'e' to exit):");
        string? message = Console.ReadLine();

        if (message?.ToLower() == "e")
        {
            break;
        }

        await rabbitMQService.PublishTopicMessageAsync(message, exchangeName, "pode2", CancellationToken.None);

        Console.WriteLine("✅ Topic message published successfully!");
    }
}

async Task ConsumeTopicMessageAsync(string _queueName)
{
    IRabbitMQConnectionFactory connectionFactory =
        new RabbitMQConnectionFactory(config, new ConsoleLogger<RabbitMQConnectionFactory>());

    IRabbitMQChannelFactory channelFactory =
        new RabbitMQChannelFactory(connectionFactory, config, new ConsoleLogger<RabbitMQChannelFactory>());

    IRabbitMQService rabbitMQService = new RabbitMQService(channelFactory, new ConsoleLogger<RabbitMQService>());

    Console.WriteLine("Topic Consumer Started!");
    Console.WriteLine($"Queue name: {_queueName}");
    Console.WriteLine($"Exchange: {exchangeName}");
    Console.WriteLine("Routing Key Pattern: pode2");
    Console.WriteLine("\nWaiting for messages...\n");

    await rabbitMQService.ConsumeTopicMessagesAsync<object>(_queueName, exchangeName, "pode2", message =>
    {
        Console.WriteLine($"📨 Received message: {message}");
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