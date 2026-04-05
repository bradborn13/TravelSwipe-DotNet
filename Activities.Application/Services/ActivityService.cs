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
                if (deserialized != null)
                    return deserialized;
            }
            var activityList = await _repository.GetActivitiesByCity(city);
            if (activityList.Count() > 0)
            {
                var slugHelper = new SlugHelper();
                var json = JsonSerializer.Serialize(activityList);
                await _cache.SetStringAsync(cacheKey, json,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    });
                List<ActivityDto> mappedResponse = _mapper.Map<List<ActivityDto>>(activityList);
                _logger.LogInformation(
               "PushingEvent ActivitiesByLocationEvent for location {DisplayName}, {EventCount} activities found",
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

                using (_metrics.ActivityScraptingEventsDuration.NewTimer())
                {
                    await _publishEndpoint.Publish(new ScrapeLocationActivitiesEvent
                    {
                        Location = city
                    });
                    return new List<ActivityDto>();
                    //return await this.ScrapeActivities(city);
                }

                //return new List<ActivityDto>();
            }
        }

        //public async Task ScrapeCityAndCountryForActivity()
        //{
        //    var activityList = await _repository.GetUniqueCityList();

        //    var citylist = activityList.Select(x => x.City).ToList();

        //    var citiesNotIncluded = await _cityRepository.FindCitiesNotRegisterd(citylist);
        //    if (citiesNotIncluded.Count() == 0)
        //    {
        //        return;
        //    }
        //    var geoLocationList = await _nominatimService.FetchLocationInfo(activityList);
        //    var slugHelper = new SlugHelper();

        //    //TODO: checked if the city is registerd, if not return them add them locally but also send event
        //    // var cityNamesList = geoLocationList
        //    //     .Select(x =>  slugHelper.GenerateSlug(x?.Address?.City))            
        //    //     .ToList();
        //    //const newCities = await _cityRepository.FindNotRegisteredCities(cityList);

        //    // var countryNameList = geoLocationList
        //    //   .Select(x => slugHelper.GenerateSlug(x.Address?.City ))
        //    //   .ToList();

        //    var cityList = geoLocationList
        //        .Select(x => new City
        //        {
        //            Country = x.Address?.Country ?? "",
        //            AssociatedNames = new List<string> { x?.Address?.City ?? "Unknown" },
        //            AssociatedSlugs = new List<string> { slugHelper.GenerateSlug(x?.Address?.City ?? "Unknown") },
        //            DisplayName = slugHelper.GenerateSlug(x?.Address?.City ?? string.Empty)
        //        })
        //        .ToList();
        //    var countryList = geoLocationList
        //     .Select(x => new Country
        //     {
        //         DisplayName = x.Address?.Country ?? string.Empty,
        //         AssociatedSlugs = new List<string> { slugHelper.GenerateSlug(x?.Address?.Country ?? string.Empty) },
        //         AssociatedNames = new List<string> { x?.Address?.Country ?? string.Empty },
        //         CountryCode = x?.Address?.CountryCode ?? ""
        //     })
        //     .ToList();
        //    await _cityRepository.AddBatch(cityList);
        //    await _countryRepository.AddBatch(countryList);
        //    foreach (var geo in geoLocationList)
        //    {
        //        await _publishEndpoint.Publish(new CityRegisteredEvent
        //        {
        //            DisplayName = geo.Address?.City ?? "",
        //            SlugList = [slugHelper.GenerateSlug(geo?.Address?.City ?? ""),],
        //            Country = geo?.Address?.Country ?? "",
        //            Municipality = geo?.Address?.Municipality ?? "",
        //            State = geo?.Address?.State?.Split(new[] { ",", "-" }, StringSplitOptions.RemoveEmptyEntries).ToList(),
        //            Postcode = geo?.Address?.Postcode ?? "",
        //            DiscoveredAt = DateTime.UtcNow,
        //            NameList = []
        //        });

        //        await _publishEndpoint.Publish(new CountryRegisteredEvent
        //        {
        //            DisplayName = geo?.Address?.Country ?? "",
        //            CountryCode = geo?.Address?.CountryCode ?? "",
        //            SlugList = [slugHelper.GenerateSlug(geo?.Address?.Country ?? "")],
        //            DiscoveredAt = DateTime.UtcNow
        //        });
        //    }

        //}


    }
}
