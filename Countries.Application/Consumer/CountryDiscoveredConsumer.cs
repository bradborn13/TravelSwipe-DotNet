using AutoMapper;
using Countries.Application.Services;
using Countries.Core.Features.Countries;
using MassTransit;
using Microsoft.Extensions.Logging;
using TravelSwipe.Contracts.Contracts;

namespace Countries.Application.Consumer
{

    public class CountryDiscoveredConsumer : IConsumer<CountryDiscoveredEvent>
    {
        private readonly ILogger<CountryDiscoveredEvent> _logger;
        private readonly CountryService _countryService;
        private readonly IMapper _mapper;

        public CountryDiscoveredConsumer(
            ILogger<CountryDiscoveredEvent> logger,
            CountryService countryService,
            IMapper mapper)
        {
            _logger = logger;
            _countryService = countryService;
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<CountryDiscoveredEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Consuming CountryDiscoveredEvent for {DisplayName}",
                @event.DisplayName
            );

            var mappedCountry = _mapper.Map<Country>(@event);
            if (mappedCountry is null)
            {
                _logger.LogWarning("Failed to map CountryDiscoveredEvent for {DisplayName}. Skipping.", @event.DisplayName);
                return;
            }

            await _countryService.AddCountries(new List<Country> { mappedCountry });
        }
    }
}
