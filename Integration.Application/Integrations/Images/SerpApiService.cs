using AutoMapper;
using Integration.Application.Integrations.Location;
using Integration.Core.Features.Activities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
namespace Integration.Application.Integrations.Images
{
    public class SerpResponse
    {
        public List<OrganicResult>? images_results { get; set; }
    }
    public class OrganicResult
    {
        public string? Original { get; set; }
        public int? Original_height { get; set; }
        public int? Original_width { get; set; }
        public string? Thumbnail { get; set; }
        public string? Source { get; set; }
        public string? Link { get; set; }
        public string? Title { get; set; }
        public int? Position { get; set; }
    };


    public class SerpApiService : ISerpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SerpApiService> _logger;
        private readonly string _apiKey;

        public SerpApiService(HttpClient httpClient, ILogger<SerpApiService> logger, IOptions<SerpApiOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = options.Value.ApiKey;
        }
        public static ImageURL MapToImage(OrganicResult dto)
        {
            return new ImageURL
            {
                ImgSource = dto.Source,
                Title = dto.Title,
                Source = dto.Source,
                Link = dto.Link,
                OriginalHeight = dto.Original_height,
                OriginalWidth = dto.Original_width,
                Thumbnail = dto.Original,
                Position = dto.Position,
            };
        }
        public async Task<List<ImageURL>> GetImages(string activityName, string city)
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["engine"] = "google_images",
                ["q"] = activityName,
                ["location"] = city,
                ["gl"] = "us",
                ["hl"] = "en",
                ["api_key"] = _apiKey,
            };
            var url = QueryHelpers.AddQueryString("search.json", queryParams);

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);

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
