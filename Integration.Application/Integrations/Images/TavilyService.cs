using Integration.Application.Integrations.Activities;
using Integration.Core.Features.Activities;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Integration.Application.Integrations.Images
{
    public class TavilyService : ITavilyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TavilyService> _logger;

        public TavilyService(HttpClient httpClient, ILogger<TavilyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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
        public async Task<List<string>> GetImages(string activityName, string city)
        {
            _logger.LogInformation("Consuming Tavily Image request, activity is {activityName} location is {City}", activityName, city);

            var url = "/search";
            var payload = new
            {
                query = $"{activityName} {city}",
                search_depth = "advanced",
                include_images = true
            };
            var response = await _httpClient.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TavilyResponse>();
            _logger.LogInformation("Tavily Image request found {count} activities for location {activityName}", result?.Images?.Count, activityName);

            return result?.Images ?? [];
        }
    }
}

