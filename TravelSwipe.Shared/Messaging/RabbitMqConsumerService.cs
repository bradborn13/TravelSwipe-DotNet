using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TravelSwipe.Shared.Messaging
{
    /// <summary>
    /// Consumes <typeparamref name="TMessage"/> from one queue and dispatches each message to
    /// <typeparamref name="THandler"/> in its own DI scope. Success acks; a handler exception
    /// copies the message to "&lt;queue&gt;_error" and acks, so a poison message never loops.
    /// </summary>
    public sealed class RabbitMqConsumerService<TMessage, THandler> : BackgroundService
        where TMessage : class
        where THandler : IMessageHandler<TMessage>
    {
        private const ushort PrefetchCount = 10;

        private readonly string _queue;
        private readonly IConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private IChannel? _channel;

        public RabbitMqConsumerService(
            string queue,
            IConnection connection,
            IServiceScopeFactory scopeFactory,
            ILogger<RabbitMqConsumerService<TMessage, THandler>> logger)
        {
            _queue = queue;
            _connection = connection;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: PrefetchCount, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += (_, ea) => OnMessageAsync(ea, stoppingToken);

            await _channel.BasicConsumeAsync(_queue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            _logger.LogInformation("Consuming {MessageType} from {Queue} with {Handler}",
                typeof(TMessage).Name, _queue, typeof(THandler).Name);

            await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default);
        }

        private async Task OnMessageAsync(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
        {
            var channel = _channel!;
            try
            {
                var message = JsonSerializer.Deserialize<TMessage>(ea.Body.Span);
                if (message is null)
                {
                    _logger.LogWarning("Received empty {MessageType} on {Queue}. Skipping.", typeof(TMessage).Name, _queue);
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    return;
                }

                await using var scope = _scopeFactory.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<MessageContext>();
                context.Queue = _queue;
                context.Exchange = ea.Exchange;
                context.RoutingKey = ea.RoutingKey;
                context.MessageType = ea.BasicProperties.Type;
                context.MessageId = ea.BasicProperties.MessageId;
                context.PublishedAt = ea.BasicProperties.IsTimestampPresent()
                    ? DateTimeOffset.FromUnixTimeSeconds(ea.BasicProperties.Timestamp.UnixTime)
                    : null;

                var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                await handler.HandleAsync(message, stoppingToken);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Shutting down: put the message back for the next instance.
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed handling {MessageType} from {Queue}. Moving it to {ErrorQueue}.",
                    typeof(TMessage).Name, _queue, _queue + MessagingTopology.ErrorQueueSuffix);
                await MoveToErrorQueueAsync(channel, ea, ex);
            }
        }

        private async Task MoveToErrorQueueAsync(IChannel channel, BasicDeliverEventArgs ea, Exception ex)
        {
            try
            {
                var headers = ea.BasicProperties.Headers is null
                    ? new Dictionary<string, object?>()
                    : new Dictionary<string, object?>(ea.BasicProperties.Headers);
                headers["x-exception-type"] = ex.GetType().FullName;
                headers["x-exception-message"] = ex.Message;
                headers["x-original-routing-key"] = ea.RoutingKey;

                var properties = new BasicProperties(ea.BasicProperties) { Headers = headers, Persistent = true };

                // Default exchange routes straight to the queue named by the routing key.
                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: _queue + MessagingTopology.ErrorQueueSuffix,
                    mandatory: false,
                    basicProperties: properties,
                    body: ea.Body);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception moveEx)
            {
                _logger.LogError(moveEx, "Could not move message to error queue; requeueing it on {Queue}.", _queue);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            if (_channel is not null)
            {
                await _channel.CloseAsync(cancellationToken);
                await _channel.DisposeAsync();
            }
        }
    }
}
