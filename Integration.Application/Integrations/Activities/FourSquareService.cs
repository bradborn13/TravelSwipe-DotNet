using AutoMapper;
using Integration.Application.Integrations.Images;
using Integration.Application.Integrations.Location;
using Integration.Core.Features.Activities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Slugify;
using System.Net.Http.Json;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Models;


namespace Integration.Application.Integrations.Activities
{
    public class FourSquareService
    {
        private readonly HttpClient _httpClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        private readonly ILogger<FourSquareService> _logger;
        private readonly INominatimService _nominatimService;
        private readonly ISerpService _serpService;



        public FourSquareService(HttpClient httpClient, IPublishEndpoint publishEndpoint, IMapper mapper, ILogger<FourSquareService> logger, INominatimService nominatimService, ISerpService serpService)
        {
            _httpClient = httpClient;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
            _logger = logger;
            _nominatimService = nominatimService;
            _serpService = serpService;
        }
        public async Task SearchPlacesAsync(string city)
        {
            try
            {
                var url = $"places/search?near={Uri.EscapeDataString(city)}";
                _logger.LogInformation("Consuming SearchPlacesAsync, location is {City}", city);
                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadFromJsonAsync<FourSquareActivities>();

                if (data is not null && data.Results is not null)
                {

                    FSActivity locationDetails = data?.Results?.FirstOrDefault(x => x.Details is not null);
                    if (locationDetails is not null)
                    {
                        CityGeoLocation looUpLocation = new CityGeoLocation { City = city, Latitude = locationDetails.Latitude, Longitude = locationDetails.Longitude };
                        await _nominatimService.FetchLocation(looUpLocation);

                    }

                    List<ActivityMQ> activityList = _mapper.Map<List<ActivityMQ>>(data.Results); ;
                    activityList.ForEach(x => x.City = city);
                    foreach (ActivityMQ activity in activityList)
                    {
                        List<ImageURL> images = await _serpService.GetImages(activity.Name, city);
                        activity.ImagesURL = _mapper.Map<List<ImageURLMQ>>(images); ;
                    }

                    await _publishEndpoint.Publish(new FoundActivitiesForLocationEvent
                    {
                        Location = city,
                        Activities = activityList
                    });

                    _logger.LogInformation("SearchPlacesAsync processed, found {ActivityCount} activities for location {City}", activityList.Count(), city);
                }
                else
                {
                    _logger.LogInformation("SearchPlacesAsync processed, found 0 activities for location {City}", city);
                    return;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SearchPlacesAsync for location {City}", city);
                throw;
            }
        }
    }
}
