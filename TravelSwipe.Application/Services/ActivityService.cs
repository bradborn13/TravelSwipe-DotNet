using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using TravelSwipe.Application.ExternalServices;
using TravelSwipe.Core;
using TravelSwipe.Core.Features.Activities;
using TravelSwipe.Core.Features.Cities;
using TravelSwipe.Core.Features.Countries;
using Slugify;
namespace TravelSwipe.Application.Services.Activities
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _repository;
        private readonly ICityRepository _cityRepository;
        private readonly ICountryRepository _countryRepository;

        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly SerpApiService _serpApiService;
        private readonly FourSquareService _foursquareService;
        private readonly NominatimAPIService _nominatimService;
        public ActivityService(IDistributedCache cache, IActivityRepository repository, ICountryRepository countryRepository, IMapper mapper, SerpApiService serpApiService, ICityRepository cityRepository, FourSquareService forsquareService, NominatimAPIService nominatimService)
        {
            _cache = cache;
            _repository = repository;
            _mapper = mapper;
            _serpApiService = serpApiService;
            _foursquareService = forsquareService;
            _nominatimService = nominatimService;
            _cityRepository = cityRepository;
            _countryRepository = countryRepository;
        }

        public async Task<List<ActivityDto>> GetActivitiesByCity(string city)
        {
            var cacheKey = $"api:activities:{city.ToLower()}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                var deserialized = JsonSerializer.Deserialize<List<ActivityDto>>(cached);
                if (deserialized != null)
                    return deserialized;
            }
            var activityList = await _repository.GetActivitiesByCity(city);
            if (activityList.Count() > 0)
            {
                var json = JsonSerializer.Serialize(activityList);
                await _cache.SetStringAsync(cacheKey, json,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    });
                return _mapper.Map<List<ActivityDto>>(activityList);
            }
            else
            {
                return await this.ScrapeActivities(city);
            }
        }
        private async Task<List<ActivityDto>> ScrapeActivities(string city)
        {
            var scrapedActivities = await _foursquareService.SearchPlacesAsync(city);
            if (scrapedActivities == null || !scrapedActivities.Any())
            {
                return [];
            }
            var mapped = _mapper.Map<List<Activity>>(scrapedActivities);
            mapped.ForEach(a => a.City = city);
            await _repository.InsertActivityBatch(mapped);
            var activityList = await _repository.GetActivitiesByCity(city);
            return _mapper.Map<List<ActivityDto>>(activityList) ?? new List<ActivityDto>();
        }
        public async Task<List<ActivityDto>> ScrapePhotosForActivity(string city)
        {
            var activitiesWithoutImages = await _repository.GetActivitiesWithoutImages(city);
            if (activitiesWithoutImages == null || activitiesWithoutImages.Count() > 0)
                return [];
            foreach (var activiti in activitiesWithoutImages)
            {
                var externalImages = await _serpApiService.GetImages(activiti.Name, city);
                await _repository.AddImage(activiti.Name, city, externalImages);
            }
            return [];
        }
        public async Task ScrapeCityAndCountryForActivity()
        {
            var activityList = await _repository.GetUniqueCountryList();

            var citylist = activityList.Select(x => x.City).ToList();

            var citiesNotIncluded = await _cityRepository.FindCitiesNotRegisterd(citylist);
            if (citiesNotIncluded.Count() == 0)
            {
                return;
            }
            var geoLocationList = await _nominatimService.FetchLocationInfo(activityList);
            var slugHelper = new SlugHelper();

            var cityList = geoLocationList
                .Select(x => new City
                {
                    
                    Country = x.Address?.Country ?? "",
                    Municipality = x.Address?.Municipality ?? "",
                    NameClean = slugHelper.GenerateSlug(x?.Address?.City ?? string.Empty),
                    AssociatedNames = new List<string> { x?.Address?.City ?? "Unknown" },
                    Postcode = x?.Address?.Postcode ?? "",
                    State = x?.Address?.State?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>(),
                })
                .ToList();
            var countryList = geoLocationList
             .Select(x => new Country
             {
                 NameClean = slugHelper.GenerateSlug(x.Address?.Country ?? string.Empty),
                 CountryCode = x.Address?.CountryCode ?? "",
                 AssociatedNames= { x?.Address?.Country ?? "" },
             })
             .ToList();
            await _cityRepository.AddBatch(cityList);
            await _countryRepository.AddBatch(countryList);

        }


    }
}
