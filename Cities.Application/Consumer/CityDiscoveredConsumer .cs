using AutoMapper;
using Cities.Application.Services;
using Cities.Core.Features.Cities;
using MassTransit;
using Microsoft.Extensions.Logging;
using TravelSwipe.Contracts.Contracts;

namespace Cities.Application.Consumers;

public class CityDiscoveredConsumer : IConsumer<CityRegisteredEvent>
{
    private readonly ILogger<CityRegisteredEvent> _logger;
    private readonly ICityService _cityService;
    private readonly IMapper _mapper;

    public CityDiscoveredConsumer(
        ILogger<CityRegisteredEvent> logger,
        ICityService cityService,
        IMapper mapper)
    {
        _logger = logger;
        _cityService = cityService;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<CityRegisteredEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Consuming CityDiscoveredEvent for {Municipality}, {Country}",
            @event.Municipality,
            @event.Country
        );

        var mappedCity = new City
        {
            DisplayName = @event.DisplayName ?? string.Empty,
            Country = @event.Country,
            Municipality = @event.Municipality,
            Postcode = @event.Postcode,
            State = @event.State,
            AssociatedSlugs = new List<string> { @event.SlugName },
            AssociatedNames = new List<string> { @event.DisplayName ?? string.Empty }
        };

        if (mappedCity is null)
        {
            _logger.LogWarning("Failed to map CityDiscoveredEvent for {Municipality}. Skipping.", @event.Municipality);
            return;
        }

        await _cityService.AddCities(new List<City> { mappedCity });
    }
}