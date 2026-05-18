using AutoMapper;
using Cities.Application.Services;
using Cities.Core.Features.Cities;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using TravelSwipe.Core.Features.Activities;
using TravelSwipe.Shared.Contracts;

namespace Cities.Application.Consumers;

public class CityRegisteredConsumer : BackgroundService
//IConsumer<CityRegisteredEvent>
{
    private readonly ILogger<CityRegisteredConsumer> _logger;
    private readonly ICityService _cityService;
    private readonly IMapper _mapper;
    private readonly IServiceProvider _serviceProvider; // To resolve Scoped services
    private readonly IConnection _connection;
    private IChannel? _channel;

    public CityRegisteredConsumer(
        ILogger<CityRegisteredConsumer> logger,
        ICityService cityService,
        IMapper mapper,
        IConnection connection,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _cityService = cityService;
        _mapper = mapper;
        _connection = connection;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync("travelswipe-exchange", ExchangeType.Topic, durable: true);
        await _channel.QueueDeclareAsync("city-service-queue", durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync("city-service-queue", "travelswipe-exchange", "city.registered");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            // 1. Create a scope to resolve your Repository/Hub (since Worker is Singleton)
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ICityService>();
            //var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            try
            {
                // 2. Deserialize the message manually
                var body = ea.Body.ToArray();
                var @event = JsonSerializer.Deserialize<CityRegisteredEvent>(Encoding.UTF8.GetString(body));

                if (@event == null)
                {
                    _logger.LogInformation("CityRegisteredEvent found no Activities. Skipping.");
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }

                _logger.LogInformation(
                           "Consuming CityRegisteredEvent for {Municipality}, {Country}",
                           @event.Municipality,
                           @event.Country
                       );

                var mappedCity = new City
                {
                    DisplayName = @event.DisplayName ?? string.Empty,
                    Country = @event.Country ?? string.Empty,
                    Municipality = @event.Municipality ?? string.Empty,
                    Postcode = @event.Postcode ?? string.Empty,
                    State = @event.State ?? new List<string>(),
                    AssociatedSlugs = @event.SlugList,
                    AssociatedNames = @event.NameList,

                };

                if (mappedCity is null)
                {
                    _logger.LogWarning("Failed to map CityRegisteredEvent for {Municipality}. Skipping.", @event.Municipality);
                    return;
                }

                await _cityService.AddCities(new List<City> { mappedCity });

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
        await _channel.BasicConsumeAsync("city-service-queue", autoAck: false, consumer: consumer);

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