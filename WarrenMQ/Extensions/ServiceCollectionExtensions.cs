using Microsoft.Extensions.DependencyInjection;
using WarrenMQ.Configuration;
using WarrenMQ.Contracts;
using WarrenMQ.Factories;
using WarrenMQ.Services;

namespace WarrenMQ.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds RabbitMQ services to the specified IServiceCollection.
    /// This method registers necessary dependencies for RabbitMQ integration,
    /// including configuration, connection factory, channel factory, and the main service.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configuration">The configuration instance containing RabbitMQ settings.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddRabbitMQ(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.Configure<RabbitMQConfig>(
            configuration.GetSection(RabbitMQConfig.SectionName));

        services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();
        services.AddSingleton<IRabbitMQChannelFactory, RabbitMQChannelFactory>();
        services.AddScoped<IRabbitMQService, RabbitMQService>();

        return services;
    }
}