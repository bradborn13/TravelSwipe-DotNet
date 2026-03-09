using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Features.Activities;
using TravelSwipe.Core.Features.Cities;
using TravelSwipe.Core.Features.Countries;

namespace TravelSwipe.Application.ExternalServices
{
    public class NominatimAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUr;
        private readonly IMapper _mapper;

        public NominatimAPIService(HttpClient httpClient, IConfiguration config, IMapper mapper)
        {
            _httpClient = httpClient;
            _apiUr = config["Nominatim:BaseUrl"]!;
            _mapper = mapper;
        }
        public async Task<List<NomResponse>> FetchLocationInfo(List<CityGeoLocation> cityList)
        {
            var locationsList = new List<NomResponse>();
            foreach (var city in cityList)
            {
                var queryParams = new Dictionary<string, string?>
                {
                    ["format"] = "jsonv2",
                    ["lat"] = city.Latitude.ToString(CultureInfo.InvariantCulture),
                    ["lon"] = city.Longitude.ToString(CultureInfo.InvariantCulture)
                };

                var url = QueryHelpers.AddQueryString("reverse", queryParams);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd("TravelSwipe-App");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var nominatimResponse = await response.Content.ReadFromJsonAsync<NomResponse>();

            }
            return locationsList;
        }
    }
}
