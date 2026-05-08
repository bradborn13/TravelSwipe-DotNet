using Activities.Api.Metrics;
using Activities.Core.Features.Activities;
using Activities.Core.Features.Cities;
using Activities.Core.Features.Countries;
using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Prometheus;
using RabbitMQ.Client;
using Slugify;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Models;


namespace Activities.Application.Services.Activities
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _repository;
        private readonly ICityRepository _cityRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IChannel _channel;
        private readonly ILogger<ActivityService> _logger;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        ActivityMetrics _metrics;
        public ActivityService(ActivityMetrics metrics, ILogger<ActivityService> logger, IDistributedCache cache, IActivityRepository repository, ICountryRepository countryRepository, IMapper mapper, ICityRepository cityRepository, IChannel channel)
        {
            _metrics = metrics;
            _cache = cache;
            _repository = repository;
            _mapper = mapper;
            _cityRepository = cityRepository;
            _countryRepository = countryRepository;
            _channel = channel;
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

                if (deserialized != null)
                {
                    var payload = new ActivitiesByLocationEvent
                    {
                        Location = city,
                        Activities = deserialized ?? []
                    };
                    var activityByLocationBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
                    await _channel.BasicPublishAsync(
                        exchange: "travelswipe-exchange", routingKey: "activities.by.location", mandatory: true,
                body: activityByLocationBody
                        );
                    return deserialized;
                }
            }
            var activityList = await _repository.GetActivitiesByCity(city);
            if (activityList.Count() == 0)
            {
                _logger.LogInformation(
                 "GetActivitiesByCity - no activity events found for city:{City}, triggering scraping",
                    city);
                using (_metrics.ActivityScraptingEventsDuration.NewTimer())
                {

                    var scrapeLocationBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ScrapeLocationActivitiesEvent
                    {
                        Location = city
                    }));
                    await _channel.BasicPublishAsync(
                        exchange: "travelswipe-exchange", routingKey: "scrapeEvents.by.location", mandatory: true,
                body: scrapeLocationBody
                        );
                    return new List<ActivityDto>();
                }
            }

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
            var activitiesByLocationEvent = new ActivitiesByLocationEvent
            {
                Location = city,
                Activities = mappedResponse
            };

            var scrapingEventRequest = new ScrapeLocationActivitiesEvent
            {
                Location = city
            };
            var scrapeEventsBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(activitiesByLocationEvent));
            await _channel.BasicPublishAsync(
                        exchange: "travelswipe-exchange", routingKey: "scrapeEvents.by.location", mandatory: true,
                body: scrapeEventsBody
                        );
            //await _publishEndpoint.Publish(new CityRegisteredEvent
            //{
            //    DisplayName = activityList[0].City ?? "",
            //    SlugList = [slugHelper.GenerateSlug(activityList[0].City)],
            //    Country = activityList[0]?.Details?.Country ?? "",
            //    Municipality = activityList[0]?.Details?.Region ?? "",
            //    State = [],
            //    Postcode = activityList[0]?.Details?.Postcode ?? "",
            //    DiscoveredAt = DateTime.UtcNow,
            //    NameList = [activityList[0].City]

            //});
            if (!string.IsNullOrEmpty(activityList[0]?.Details?.Country))
            {
                //    await _publishEndpoint.Publish(new CountryRegisteredEvent
                //    {
                //        DisplayName = activityList[0].Details?.Country ?? "",
                //        CountryCode = "",
                //        SlugList = [slugHelper.GenerateSlug(activityList[0].Details?.Country ?? "")],
                //        DiscoveredAt = DateTime.UtcNow,
                //        NameList = [activityList[0].Details?.Country ?? ""]
                //    });
            }
            _metrics.GetActivitiesViaDb.Inc();
            return mappedResponse;
        }
        public async Task TriggerIntegrationImageUpdate(string city)
        {

            var activityByNameList = await _repository.GetAllActivityNamesByLocation(city);
            if (activityByNameList == null || activityByNameList.Count == 0)
            {
                _logger.LogInformation(
           "TriggerIntegrationImageUpdate - did NOT publish event for location  {city}. No acitvities found",
           city
       );
                return;
            }
            _logger.LogInformation(
    "TriggerIntegrationImageUpdate - publishing event for location: {city}, {activitiesCount} activities found",
    city, activityByNameList.Count
);
            //await _publishEndpoint.Publish(new FetchLatestImagesForLocation
            //{
            //    City = city,
            //    Activities = activityByNameList
            //});
        }
        public async Task UpdateImagesOnActivities(string city, Dictionary<string, List<ImageURLMQ>> imagePackageByActivities)
        {
            try
            {

                var tasks = imagePackageByActivities.Select(async activity =>
                {
                    try
                    {
                        return await _repository.UpdateImages(city, activity.Key, _mapper.Map<List<ImageURL>>(activity.Value));


                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update images for activity: {activity} on city: {city}", activity.Key, city);
                        return 0;
                    }
                }
                );
                var results = await Task.WhenAll(tasks);

                var totalUpdated = results.Sum();
                if (totalUpdated > 0)
                {
                    var cacheKey = $"api:activities:{city.ToLower()}";
                    await _cache.RemoveAsync(cacheKey);
                }
                _logger.LogInformation("Updated {total} records for {count} activities in {city}",
                     totalUpdated, imagePackageByActivities.Count, city);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UpdateImagesOnActivities for location {City}, exception:{exception}", city, ex.Message);
                throw;
            }
        }
    }
}

