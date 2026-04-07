using Activities.Api.Metrics;
using Activities.Application.Consumer;
using Activities.Core.Features.Activities;
using Activities.Core.Features.Cities;
using Activities.Core.Features.Countries;
using AutoMapper;
using MassTransit;
using MassTransit.Middleware;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Prometheus;
using Slugify;
using System.Text.Json;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Models;

namespace Activities.Application.Services.Activities
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _repository;
        private readonly ICityRepository _cityRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ActivityService> _logger;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        ActivityMetrics _metrics;
        public ActivityService(ActivityMetrics metrics, ILogger<ActivityService> logger, IDistributedCache cache, IActivityRepository repository, ICountryRepository countryRepository, IMapper mapper, ICityRepository cityRepository, IPublishEndpoint publishEndpoint)
        {
            _metrics = metrics;
            _cache = cache;
            _repository = repository;
            _mapper = mapper;
            _cityRepository = cityRepository;
            _countryRepository = countryRepository;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<List<ActivityDto>> GetActivitiesByCity(string city)
        {
            var cacheKey = $"api:activities:{city.ToLower()}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _metrics.GetActivitiesViaReddis.Inc();
                var deserialized = JsonSerializer.Deserialize<List<ActivityDto>>(cached);
                await _publishEndpoint.Publish(new ActivitiesByLocationEvent
                {
                    Location = city,
                    Activities = deserialized
                });
                if (deserialized != null)
                    return deserialized;
            }
            var activityList = await _repository.GetActivitiesByCity(city);
            if (activityList.Count() > 0)
            {
                _logger.LogInformation(
              "GetActivitiesByCity - Found and returning {EventCount} activity events for city:{City}",
                  activityList.Count(), city
          );
                var slugHelper = new SlugHelper();
                List<ActivityDto> mappedResponse = _mapper.Map<List<ActivityDto>>(activityList);
                var json = JsonSerializer.Serialize(mappedResponse);
                await _cache.SetStringAsync(cacheKey, json,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    });
                _logger.LogInformation(
               "GetActivitiesByCity - PushingEvent ActivitiesByLocationEvent for location {DisplayName}, {EventCount} activities found",
               city, mappedResponse.Count()
           );
                await _publishEndpoint.Publish(new ActivitiesByLocationEvent
                {
                    Location = city,
                    Activities = mappedResponse
                });
                await _publishEndpoint.Publish(new CityRegisteredEvent
                {
                    DisplayName = activityList[0].City ?? "",
                    SlugList = [slugHelper.GenerateSlug(activityList[0].City)],
                    Country = activityList[0]?.Details?.Country ?? "",
                    Municipality = activityList[0]?.Details?.Region ?? "",
                    State = [],
                    Postcode = activityList[0]?.Details?.Postcode ?? "",
                    DiscoveredAt = DateTime.UtcNow,
                    NameList = [activityList[0].City]

                });
                if (!string.IsNullOrEmpty(activityList[0]?.Details?.Country))
                {
                    await _publishEndpoint.Publish(new CountryRegisteredEvent
                    {
                        DisplayName = activityList[0].Details?.Country ?? "",
                        CountryCode = "",
                        SlugList = [slugHelper.GenerateSlug(activityList[0].Details?.Country ?? "")],
                        DiscoveredAt = DateTime.UtcNow,
                        NameList = [activityList[0].Details?.Country ?? ""]
                    });
                }
                _metrics.GetActivitiesViaDb.Inc();
                return mappedResponse;
            }
            else
            {
                _logger.LogInformation(
                   "GetActivitiesByCity - no activity events found for city:{City}, triggering scraping",
                      city);
                using (_metrics.ActivityScraptingEventsDuration.NewTimer())
                {
                    await _publishEndpoint.Publish(new ScrapeLocationActivitiesEvent
                    {
                        Location = city
                    });
                    return new List<ActivityDto>();
                }

            }
        }

    }
}
