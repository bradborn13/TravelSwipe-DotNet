using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TravelSwipe.Activities.Core.Features.Activities;

namespace TravelSwipe.Application.ExternalServices
{
    public class FourSquareService
    {
        private readonly HttpClient _httpClient;

        public FourSquareService(HttpClient httpClient)
        {
            _httpClient = httpClient;

        }
        public async Task<List<FSActivity>> SearchPlacesAsync(string city)
        {
            var url = $"places/search?near={Uri.EscapeDataString(city)}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<FourSquareActivities>();

            if (data?.Results == null || !data.Results.Any())
            {
                return new List<FSActivity>();
            }


            return data.Results;
        }
    }
}
