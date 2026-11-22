using AutoMapper;
using Countries.Application.Services;
using Countries.Core.Features.Countries;
using TravelSwipe.Shared.Messaging;
using Microsoft.Extensions.Logging;
using TravelSwipe.Shared.Contracts;

namespace Countries.Application.Consumer
{

    public class CountryRegisteredConsumer : IMessageHandler<CountryRegisteredEvent>
    {
        private readonly ILogger<CountryRegisteredConsumer> _logger;
        private readonly ICountryService _countryService;
        private readonly IMapper _mapper;

        public CountryRegisteredConsumer(
            ILogger<CountryRegisteredConsumer> logger,
            ICountryService countryService,
            IMapper mapper)
        {
            _logger = logger;
            _countryService = countryService;
            _mapper = mapper;
        }

        public async Task HandleAsync(CountryRegisteredEvent @event, CancellationToken cancellationToken)
        {

            _logger.LogInformation(
                "Consuming CountryDiscoveredEvent for {DisplayName}",
                @event.DisplayName
            );

            var mappedCountry = new Country
            {
                DisplayName = @event.DisplayName,
                AssociatedNames = @event.NameList,
                AssociatedSlugs = @event.SlugList,
                CountryCode = @event.CountryCode ?? string.Empty
            };
            if (mappedCountry is null)
            {
                _logger.LogWarning("Failed to map CountryDiscoveredEvent for {DisplayName}. Skipping.", @event.DisplayName);
                return;
            }
            await _countryService.AddCountries(new List<Country> { mappedCountry });
        }
    }
}
