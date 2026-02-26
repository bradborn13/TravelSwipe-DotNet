using Microsoft.Extensions.Configuration;
using SerpApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Features.Activities;
namespace TravelSwipe.Application.ExternalServices
{
    public class SerpResponse
    {
        public List<OrganicResult>? images_results { get; set; }
    }
    public class OrganicResult
    {
        public string? Original { get; set; }
        public string? Original_height { get; set; }
        public double? Original_width { get; set; }
        public string? Thumbnail { get; set; }
        public string? Title { get; set; }
        public string? Source { get; set; }
        public string? Link { get; set; }
        public string? Position { get; set; }
    }
    public class SerpApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        public SerpApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["SerpApi:ApiKey"]!;
        }
        public static ImageURL MapToImage(OrganicResult dto)
        {
            return new ImageURL
            {
                ImgSource = dto.Source,
                Title = dto.Title,
                Source = dto.Original,
                Link = dto.Link,
                OriginalHeight = double.Parse(dto.Original_height),
                OriginalWidth = dto.Original_width,
                Thumbnail = dto.Thumbnail,
                Position = int.Parse(dto.Position)

            };
        }
        public async Task<List<ImageURL>> GetImages(string activityName, string city)
        {
            var url =
             $"search.json?engine=google_images" +
             $"&q={Uri.EscapeDataString(activityName)}" +
             $"&location={Uri.EscapeDataString(city)}" +
             $"&gl=us" +
             $"&hl=en" +
             $"&api_key={_apiKey}";

            //var url = $"search.json?q={Uri.EscapeDataString(city)}&api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"SerpApi failed: {error}");
            }

            var serpResponse = await response.Content.ReadFromJsonAsync<SerpResponse>();
            var mappedData = serpResponse?.images_results?.Select(x => MapToImage(x)).ToList();
            return mappedData ?? [];
        }
    }

}
