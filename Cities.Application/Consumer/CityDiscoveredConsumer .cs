using AutoMapper;
using Cities.Application.Services;
using Cities.Core.Features.Cities;
using MassTransit;
using Microsoft.Extensions.Logging;
using TravelSwipe.Contracts.Contracts;

namespace Cities.Application.Consumers;

public class CityDiscoveredConsumer : IConsumer<CityDiscoveredEvent>
{
    private readonly ILogger<CityDiscoveredConsumer> _logger;
    private readonly CityService _cityService;
    private readonly IMapper _mapper;

    public CityDiscoveredConsumer(
        ILogger<CityDiscoveredConsumer> logger,
        CityService cityService,
        IMapper mapper)
    {
        _logger = logger;
        _cityService = cityService;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<CityDiscoveredEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Consuming CityDiscoveredEvent for {Municipality}, {Country}",
            @event.Municipality,
            @event.Country
        );

        var mappedCity = _mapper.Map<City>(@event);
        if (mappedCity is null)
        {
            _logger.LogWarning("Failed to map CityDiscoveredEvent for {Municipality}. Skipping.", @event.Municipality);
            return;
        }

        await _cityService.AddCities(new List<City> { mappedCity });
    }
}