using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace TravelSwipe.Shared.Messaging
{
    public static class RabbitMqServiceCollectionExtensions
    {
        private const int ConnectAttempts = 10;
        private static readonly TimeSpan ConnectRetryDelay = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Registers the RabbitMQ connection, <see cref="IEventPublisher"/>, and a startup step that
        /// declares <see cref="MessagingTopology"/>. Reads RabbitMQ:Host, RabbitMQ:Username, RabbitMQ:Password.
        /// Call before any <see cref="AddRabbitMqConsumer{TMessage, THandler}"/>.
        /// </summary>
        public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"] ?? "rabbitmq",
                UserName = configuration["RabbitMQ:Username"] ?? "admin",
                Password = configuration["RabbitMQ:Password"] ?? "secretpassword",
                ClientProvidedName = AppDomain.CurrentDomain.FriendlyName,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
            };

            services.AddSingleton<IConnection>(sp =>
                ConnectWithRetryAsync(factory, sp.GetRequiredService<ILoggerFactory>().CreateLogger("RabbitMQ"))
                    .GetAwaiter().GetResult());
            services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
            services.AddScoped<MessageContext>();

            // Hosted services start in registration order, so the topology exists before any consumer starts.
            services.AddHostedService<RabbitMqTopologyInitializer>();
            return services;
        }

        /// <summary>
        /// Consumes <typeparamref name="TMessage"/> from <paramref name="queue"/> and dispatches each
        /// message to a scoped <typeparamref name="THandler"/>.
        /// </summary>
        public static IServiceCollection AddRabbitMqConsumer<TMessage, THandler>(this IServiceCollection services, string queue)
            where TMessage : class
            where THandler : class, IMessageHandler<TMessage>
        {
            services.AddScoped<THandler>();
            services.AddSingleton<IHostedService>(sp => new RabbitMqConsumerService<TMessage, THandler>(
                queue,
                sp.GetRequiredService<IConnection>(),
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<ILogger<RabbitMqConsumerService<TMessage, THandler>>>()));
            return services;
        }

        private static async Task<IConnection> ConnectWithRetryAsync(ConnectionFactory factory, ILogger logger)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await factory.CreateConnectionAsync();
                }
                catch (Exception ex) when (attempt < ConnectAttempts)
                {
                    logger.LogWarning(ex, "RabbitMQ connection attempt {Attempt}/{Max} to {Host} failed; retrying in {Delay}s",
                        attempt, ConnectAttempts, factory.HostName, ConnectRetryDelay.TotalSeconds);
                    await Task.Delay(ConnectRetryDelay);
                }
            }
        }
    }

    internal sealed class RabbitMqTopologyInitializer : IHostedService
    {
        private readonly IConnection _connection;

        public RabbitMqTopologyInitializer(IConnection connection)
        {
            _connection = connection;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await MessagingTopology.DeclareAsync(channel, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
