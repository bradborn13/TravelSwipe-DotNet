using Integration.Application.Integrations.Images;
using Integration.Core.Features.Activities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace Integration.Tests
{
    public class SerpApiServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly SerpApiService _service;

        public SerpApiServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://serpapi.com/")
            };
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<SerpApiService>();
            var options = Options.Create(new SerpApiOptions
            {
                ApiKey = "test-key"
            });
            //_mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), loggerFactory));

            var serpMock = new Mock<ISerpService>();
            _service = new SerpApiService(
                _httpClient,
                logger,
                options

                );
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
        [Fact]
        public async Task SearchPlacesAsync_WithValidResponse_PulishesEventWithMbappedActivities()
        {

            SerpResponse mockSerpApiResponse = new SerpResponse() { images_results = new List<OrganicResult> { new OrganicResult() { Link = "link_mock", Original = "original_mock", Original_height = 333, Original_width = 2222, Position = 1, Title = "title_mock", Source = "source_mock", Thumbnail = "thumbnail_mock" }, new OrganicResult() { Link = "link_mock_2", Original = "original_mock_2", Original_height = 333, Original_width = 2222, Position = 1, Title = "title_mock_2", Source = "source_mock_2", Thumbnail = "thumbnail_mock_2" } } };

            var jsonContent = JsonSerializer.Serialize(mockSerpApiResponse);

            // Mock HTTP response
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock
                .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    .ReturnsAsync(httpResponse);


            // Act
            var imagesResponse = await _service.GetImages("Crab Restaurant", "Seul");
            Assert.NotNull(imagesResponse);
            mockSerpApiResponse.images_results.ForEach(mockImage =>
            {
                var index = mockSerpApiResponse.images_results.IndexOf(mockImage);
                var imageMapped = MapToImage(mockImage);
                var imageMappedAsJson = JsonSerializer.Serialize(imageMapped);
                var responseAsJson = JsonSerializer.Serialize(imagesResponse[index]);
                Assert.Equal(imageMappedAsJson, responseAsJson);
            });
        }
    }
}
