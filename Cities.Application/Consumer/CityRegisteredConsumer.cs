using Cities.Application.Services;
using Cities.Core.Features.Cities;
using Microsoft.Extensions.Logging;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Messaging;

namespace Cities.Application.Consumers;

public class CityRegisteredConsumer : IMessageHandler<CityRegisteredEvent>
{
    private readonly ILogger<CityRegisteredConsumer> _logger;
    private readonly ICityService _cityService;

    public CityRegisteredConsumer(ILogger<CityRegisteredConsumer> logger, ICityService cityService)
    {
        _logger = logger;
        _cityService = cityService;
    }

    public async Task HandleAsync(CityRegisteredEvent @event, CancellationToken cancellationToken)
    {
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

        await _cityService.AddCities(new List<City> { mappedCity });
    }
}
