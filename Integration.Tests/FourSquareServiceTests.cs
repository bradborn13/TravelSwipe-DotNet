using AutoMapper;
using Castle.Core.Logging;
using Infrastructure.Mappings;
using Integration.Application.Integrations.Activities;
using Integration.Application.Integrations.Images;
using Integration.Application.Integrations.Location;
using Integration.Core.Features.Activities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using TravelSwipe.Shared.Contracts;
using Xunit.Abstractions;

namespace Integration.Application.Tests
{
    public class FourSquareServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<FourSquareService>> _loggerMock;
        private readonly FourSquareService _service;

        public FourSquareServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.foursquare.com/")
            };

            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _loggerMock = new Mock<ILogger<FourSquareService>>();
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<FourSquareService>();


            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), loggerFactory));

            var nominatimMock = new Mock<INominatimService>();
            var serpMock = new Mock<ISerpService>();
            _service = new FourSquareService(
                _httpClient,
                _publishEndpointMock.Object,
                _mapper,
                _loggerMock.Object, nominatimMock.Object, serpMock.Object
                );
        }

        [Fact]
        public async Task SearchPlacesAsync_WithValidResponse_PulishesEventWithMbappedActivities()
        {
            // Arrange
            var city = "Copenhagen";
            List<FSActivity> fsActivities = new List<FSActivity>
            {
                new FSActivity {  Name = "Tivoli", Latitude = 55.6761, Longitude = 12.5683 ,DateCreated= Convert.ToString( DateTime.Now),DateRefreshed=Convert.ToString( DateTime.Now), FsqId="4123", Details = new FSLocation{ Region="Midjutland",Postcode="412321",Country="DK",Locality="Aar",Address="none",FormattedAddress="nope"} },
                new FSActivity { Name = "Nyhavn", Latitude = 55.6794, Longitude = 12.5903 ,DateCreated= Convert.ToString( DateTime.Now),DateRefreshed=Convert.ToString( DateTime.Now),FsqId="52352" }
            };



            FourSquareActivities apiResponse = new FourSquareActivities
            {
                Results = fsActivities
            };

            var jsonContent = JsonSerializer.Serialize(apiResponse);

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




            // Mock publish endpoint
            var publishedEvents = new List<object>();

            //_publishEndpointMock
            //   .Setup(p => p.Publish(It.IsAny<CityRegisteredEvent>(), It.IsAny<CancellationToken>()))
            //   .Callback<CityRegisteredEvent, CancellationToken>((evt, ct) => publishedEvents.Add(evt))
            //   .Returns(Task.CompletedTask);

            _publishEndpointMock
                .Setup(p => p.Publish(It.IsAny<FoundActivitiesForLocationEvent>(), It.IsAny<CancellationToken>()))
                .Callback<FoundActivitiesForLocationEvent, CancellationToken>((evt, ct) => publishedEvents.Add(evt))
                .Returns(Task.CompletedTask);

            // Act
            await _service.SearchPlacesAsync(city);

            // Assert
            //CityRegisteredEvent foundCity = publishedEvents[0] as CityRegisteredEvent;
            //Assert.NotNull(foundCity);
            //Assert.Equal(city, foundCity.DisplayName);

            Assert.NotEmpty(publishedEvents);
            FoundActivitiesForLocationEvent foundActivitiesEvents = publishedEvents[0] as FoundActivitiesForLocationEvent;
            Assert.Equal(2, foundActivitiesEvents.Activities.Count);
            Assert.NotNull(foundActivitiesEvents);

            Assert.Equal(city, foundActivitiesEvents.Location);
            Assert.Equal(2, foundActivitiesEvents.Activities.Count);
            Assert.Equal("Tivoli", foundActivitiesEvents.Activities[0].Name);
        }

        [Fact]
        public async Task SearchPlacesAsync_WithEmptyResults_PublishesEventWithEmptyActivities()
        {
            // Arrange
            var city = "UnknownCity";
            var apiResponse = new FourSquareActivities { Results = new List<FSActivity>() };
            var jsonContent = JsonSerializer.Serialize(apiResponse);

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
            await _service.SearchPlacesAsync(city);

            // Assert
            _publishEndpointMock.Verify(
                p => p.Publish(
                    It.Is<FoundActivitiesForLocationEvent>(e =>
                        e.Location == city &&
                        e.Activities.Count == 0
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task SearchPlacesAsync_WithHttpError_ThrowsException()
        {
            // Arrange
            var city = "Copenhagen";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _service.SearchPlacesAsync(city));

            // Verify no event was published
            _publishEndpointMock.Verify(
                p => p.Publish(It.IsAny<FoundActivitiesForLocationEvent>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SearchPlacesAsync_WithInvalidJson_ThrowsException()
        {
            // Arrange
            var city = "Copenhagen";

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ invalid json", System.Text.Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => _service.SearchPlacesAsync(city));

            // Verify no event was published
            _publishEndpointMock.Verify(
                p => p.Publish(It.IsAny<FoundActivitiesForLocationEvent>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
}
