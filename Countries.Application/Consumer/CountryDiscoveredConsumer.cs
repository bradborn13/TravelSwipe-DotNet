using AutoMapper;
using Countries.Application.Services;
using Countries.Core.Features.Countries;
using MassTransit;
using Microsoft.Extensions.Logging;
using TravelSwipe.Contracts.Contracts;

namespace Countries.Application.Consumer
{

    public class CountryDiscoveredConsumer : IConsumer<CountryRegisteredEvent>
    {
        private readonly ILogger<CountryRegisteredEvent> _logger;
        private readonly ICountryService _countryService;
        private readonly IMapper _mapper;

        public CountryDiscoveredConsumer(
            ILogger<CountryRegisteredEvent> logger,
            ICountryService countryService,
            IMapper mapper)
        {
            _logger = logger;
            _countryService = countryService;
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<CountryRegisteredEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Consuming CountryDiscoveredEvent for {DisplayName}",
                @event.DisplayName
            );

            var mappedCountry = new Country
            {
                DisplayName = @event.DisplayName,
                AssociatedNames = new List<string> { @event.DisplayName },
                AssociatedSlugs = new List<string> { @event.SlugName },
                CountryCode = @event.CountryCode
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
