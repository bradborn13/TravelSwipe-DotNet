using AutoMapper;
using EventStore.Client;
using MassTransit;
using MassTransit.Transports.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using TravelSwipe.Shared.Contracts;
using ExchangeType = RabbitMQ.Client.ExchangeType;
using Timeout = System.Threading.Timeout;

namespace EventHub.Application.Consumers;

public class CityRegisteredConsumer : BackgroundService
{
    private readonly ILogger<CityRegisteredConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private IChannel? _channel;
    private readonly EventStoreClient eventStore;

    public CityRegisteredConsumer(
        ILogger<CityRegisteredConsumer> logger,
        IConnection connection,
        IServiceProvider serviceProvider,
         EventStoreClient _eventStore)
    {
        _logger = logger;
        _connection = connection;
        _serviceProvider = serviceProvider;
        eventStore = _eventStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync("travelswipe-exchange", ExchangeType.Topic, durable: true);
        await _channel.QueueDeclareAsync("eventhub-city-queue", durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync("eventhub-city-queue", "travelswipe-exchange", "city.registered");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            // 1. Create a scope to resolve your Repository/Hub (since Worker is Singleton)
            using var scope = _serviceProvider.CreateScope();


            try
            {
                // 2. Deserialize the message manually
                var body = ea.Body.ToArray();
                var @event = JsonSerializer.Deserialize<CityRegisteredEvent>(Encoding.UTF8.GetString(body));

                if (@event == null)
                {
                    _logger.LogInformation("EventHub - CityRegisteredEvent found no Activities. Skipping.");
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }
                _logger.LogInformation("EventHub - Publishing CityRegisteredEvent Event to EventStore.");

                var eventData = new EventData(Uuid.NewUuid(),
                                   nameof(@event),
                                   JsonSerializer.SerializeToUtf8Bytes(@event).AsMemory());

                //this.eventStore.AppendToStreamAsync()
                await this.eventStore
                            .AppendToStreamAsync(nameof(CityRegisteredEvent),
                                                  StreamState.Any,
                                                  new[] { eventData });

                // 4. Tell RabbitMQ we are done
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CityRegisteredEvent ");
                // Nack the message to requeue it or move to error queue
                await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
            }
        };
        await _channel.BasicConsumeAsync("eventhub-city-queue", autoAck: false, consumer: consumer);

        // Keep the task alive until the app shuts down
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) { await _channel.CloseAsync(); await _channel.DisposeAsync(); }
        ;
        await base.StopAsync(cancellationToken);
    }
}