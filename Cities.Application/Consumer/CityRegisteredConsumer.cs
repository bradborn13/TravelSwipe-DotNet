using AutoMapper;
using Cities.Application.Services;
using Cities.Core.Features.Cities;
using MassTransit;
using Microsoft.Extensions.Logging;
using TravelSwipe.Shared.Contracts;

namespace Cities.Application.Consumers;

public class CityRegisteredConsumer : IConsumer<CityRegisteredEvent>
{
    private readonly ILogger<CityRegisteredConsumer> _logger;
    private readonly ICityService _cityService;
    private readonly IMapper _mapper;

    public CityRegisteredConsumer(
        ILogger<CityRegisteredConsumer> logger,
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
            Country = @event.Country ?? string.Empty,
            Municipality = @event.Municipality ?? string.Empty,
            Postcode = @event.Postcode ?? string.Empty,
            State = @event.State ?? new List<string>(),
            AssociatedSlugs = @event.SlugList,
            AssociatedNames = @event.NameList,

        };

        if (mappedCity is null)
        {
            _logger.LogWarning("Failed to map CityDiscoveredEvent for {Municipality}. Skipping.", @event.Municipality);
            return;
        }

        await _cityService.AddCities(new List<City> { mappedCity });
    }
}