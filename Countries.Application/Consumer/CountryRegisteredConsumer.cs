using AutoMapper;
using Countries.Application.Services;
using Countries.Core.Features.Countries;
using MassTransit;
using Microsoft.Extensions.Logging;
using TravelSwipe.Contracts.Contracts;

namespace Countries.Application.Consumer
{

    public class CountryRegisteredConsumer : IConsumer<CountryRegisteredEvent>
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
                AssociatedNames = new List<string> { @event.DisplayName ?? string.Empty },
                AssociatedSlugs = new List<string> { @event.SlugName ?? string.Empty },
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
