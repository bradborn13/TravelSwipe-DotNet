using AutoMapper;
using Integration.Application.Integrations.Activities;
using Integration.Core.Features.Activities;
using MassTransit;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Slugify;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Contracts.Contracts;

namespace Integration.Application.Integrations.Location
{
    public class NominatimAPIService : INominatimService
    {
        private readonly HttpClient _httpClient;
        //private readonly string _apiUr;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<NominatimAPIService> _logger;

        public NominatimAPIService(HttpClient httpClient, IPublishEndpoint publishEndpoint, ILogger<NominatimAPIService> logger)
        {
            _httpClient = httpClient;
            //_apiUr = config["Nominatim:BaseUrl"];
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }
        //public async Task<List<NomResponse>> FetchLocationInfo(List<CityGeoLocation> cityList)
        //{
        //    var slugHelper = new SlugHelper();
        //    var locationsList = new List<NomResponse>();
        //    foreach (var location in cityList)
        //    {
        //        var queryParams = new Dictionary<string, string?>
        //        {
        //            ["format"] = "jsonv2",
        //            ["lat"] = location.Latitude.ToString(CultureInfo.InvariantCulture),
        //            ["lon"] = location.Longitude.ToString(CultureInfo.InvariantCulture)
        //        };

        //        var url = QueryHelpers.AddQueryString("reverse", queryParams);

        //        var request = new HttpRequestMessage(HttpMethod.Get, url);
        //        request.Headers.UserAgent.ParseAdd("TravelSwipe-App");

        //        var response = await _httpClient.SendAsync(request);
        //        response.EnsureSuccessStatusCode();

        //        var nominatimResponse = await response.Content.ReadFromJsonAsync<NomResponse>();
        //        await _publishEndpoint.Publish(new CityRegisteredEvent
        //        {
        //            Country = nominatimResponse?.Address?.Country ?? "",
        //            DiscoveredAt = DateTime.Now,
        //            DisplayName = location.City,
        //            Postcode = nominatimResponse?.Address?.Postcode,
        //            SlugList = [slugHelper.GenerateSlug(location.City),]
        //        },
        //            State = [nominatimResponse?.Address?.State ?? ""]


        //        });
        //        await _publishEndpoint.Publish(new CountryRegisteredEvent
        //        {
        //            CountryCode = nominatimResponse?.Address?.CountryCode ?? "",
        //            DiscoveredAt = DateTime.Now,
        //            DisplayName = nominatimResponse?.Address?.Country ?? "",
        //            SlugName = slugHelper.GenerateSlug(nominatimResponse?.Address?.Country ?? ""),


        //        });
        //    }
        //    return locationsList;
        //}
        public async Task FetchLocation(CityGeoLocation location)
        {
            var slugHelper = new SlugHelper();
            var locationsList = new List<NomResponse>();

            var queryParams = new Dictionary<string, string?>
            {
                ["format"] = "jsonv2",
                ["lat"] = location.Latitude.ToString(CultureInfo.InvariantCulture),
                ["lon"] = location.Longitude.ToString(CultureInfo.InvariantCulture)
            };

            var url = QueryHelpers.AddQueryString("reverse", queryParams);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("TravelSwipe-App");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var nominatimResponse = await response.Content.ReadFromJsonAsync<NomResponse>();
            if (nominatimResponse is not null && nominatimResponse.Address is not null)
            {
                List<string> slugList = [slugHelper.GenerateSlug(location.City)];
                List<string> nameList = [location.City];

                if (location.City != nominatimResponse?.Address?.City)
                {
                    slugList.Add(slugHelper.GenerateSlug(location.City));
                    nameList.Add(nominatimResponse?.Address?.City);

                }
                await _publishEndpoint.Publish(new CityRegisteredEvent
                {
                    Country = nominatimResponse.Address.Country ?? "",
                    DiscoveredAt = DateTime.Now,
                    DisplayName = location.City,
                    Postcode = nominatimResponse?.Address?.Postcode,
                    SlugList = slugList,
                    State = [nominatimResponse?.Address?.State ?? ""],
                    NameList = nameList


                });
                await _publishEndpoint.Publish(new CountryRegisteredEvent
                {
                    CountryCode = nominatimResponse?.Address?.CountryCode ?? "",
                    DiscoveredAt = DateTime.Now,
                    DisplayName = nominatimResponse?.Address?.Country ?? "",
                    SlugList = [slugHelper.GenerateSlug(nominatimResponse?.Address?.Country ?? "")],
                    NameList = [nominatimResponse?.Address?.Country ?? ""]


                });
            }

        }
    }

}
