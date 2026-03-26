using AutoMapper;
using Integration.Application.Integrations.Activities;
using Integration.Core.Features.Activities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TravelSwipe.Contracts.Contracts;
using Xunit;

namespace Integration.Application.Tests
{
    public class FourSquareServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<IMapper> _mapperMock;
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
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<FourSquareService>>();

            _service = new FourSquareService(
                _httpClient,
                _publishEndpointMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task SearchPlacesAsync_WithValidResponse_PublishesEventWithMappedActivities()
        {
            // Arrange
            var city = "Copenhagen";
            var fsActivities = new List<FSActivity>
            {
                new FSActivity {  Name = "Tivoli", Latitude = 55.6761, Longitude = 12.5683 ,DateCreated= Convert.ToString( DateTime.Now),DateRefreshed=Convert.ToString( DateTime.Now),FsqId="4123"},
                new FSActivity { Name = "Nyhavn", Latitude = 55.6794, Longitude = 12.5903 ,DateCreated= Convert.ToString( DateTime.Now),DateRefreshed=Convert.ToString( DateTime.Now),FsqId="52352" }
            };



            var apiResponse = new FourSquareActivities
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
            _publishEndpointMock
                .Setup(p => p.Publish(It.IsAny<FoundActivitiesForLocationEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.SearchPlacesAsync(city);

            // Assert
            _publishEndpointMock.Verify(
                p => p.Publish(
                    It.Is<FoundActivitiesForLocationEvent>(e =>
                        e.Location == city &&
                        e.Activities.Count == 2 &&
                        e.Activities[0].Name == "Tivoli" &&
                        e.Activities[1].Name == "Nyhavn"
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
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
