using System.Text.Json;
using RabbitMQ.Client;

namespace TravelSwipe.Shared.Messaging
{
    /// <summary>
    /// Publishes on one long-lived channel with publisher confirms. Channels are not safe for
    /// concurrent use, so publishes are serialized.
    /// </summary>
    public sealed class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private IChannel? _channel;

        public RabbitMqEventPublisher(IConnection connection)
        {
            _connection = connection;
        }

        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Type = typeof(T).Name,
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            };

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_channel is null || _channel.IsClosed)
                {
                    _channel = await _connection.CreateChannelAsync(
                        new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
                        cancellationToken);
                }

                await _channel.BasicPublishAsync(
                    exchange: MessagingTopology.Exchange,
                    routingKey: MessagingTopology.RoutingKeyFor<T>(),
                    mandatory: false,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }
            _lock.Dispose();
        }
    }
}
