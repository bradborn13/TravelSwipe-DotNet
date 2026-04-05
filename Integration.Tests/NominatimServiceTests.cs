using AutoMapper;
using Integration.Application.Integrations.Activities;
using Integration.Application.Integrations.Location;
using Integration.Core.Features.Activities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using TravelSwipe.Shared.Contracts;

namespace Integration.Tests
{


    public class NominatimServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<ILogger<NominatimAPIService>> _loggerMock;
        private readonly NominatimAPIService _service;

        public NominatimServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.foursquare.com/")
            };

            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _loggerMock = new Mock<ILogger<NominatimAPIService>>();
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<NominatimAPIService>();


            //_mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), loggerFactory));

            var nominatimMock = new Mock<INominatimService>();
            var serpMock = new Mock<ISerpService>();
            _service = new NominatimAPIService(
                _httpClient,
                _publishEndpointMock.Object,
                _loggerMock.Object
                );
        }

        [Fact]
        public async Task SearchPlacesAsync_WithValidResponse_PulishesEventWithMbappedActivities()
        {
            // Arrange
            CityGeoLocation city = new CityGeoLocation() { City = "Copenhagen", Latitude = 41221, Longitude = 451212 };
            NomResponse mockLocation = new NomResponse()
            {
                Address = new NomAddress
                {
                    City = "Copenhagen",
                    Country = "Denmark",
                    CountryCode = "51234",
                    Municipality = "Kobv",
                    Postcode = "1500",
                    State = "Jutland"
                }
            };

            var jsonContent = JsonSerializer.Serialize(mockLocation);

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
            var publishedCreatedCity = new List<CityRegisteredEvent>();
            var publishedCreatedCountry = new List<CountryRegisteredEvent>();
            _publishEndpointMock
                .Setup(p => p.Publish(It.IsAny<CityRegisteredEvent>(), It.IsAny<CancellationToken>()))
                .Callback<CityRegisteredEvent, CancellationToken>((evt, ct) => publishedCreatedCity.Add(evt))
                .Returns(Task.CompletedTask);

            _publishEndpointMock
           .Setup(p => p.Publish(It.IsAny<CountryRegisteredEvent>(), It.IsAny<CancellationToken>()))
           .Callback<CountryRegisteredEvent, CancellationToken>((evt, ct) => publishedCreatedCountry.Add(evt))
           .Returns(Task.CompletedTask);

            // Act
            await _service.FetchLocation(city);

            Assert.Single(publishedCreatedCity);
            Assert.Single(publishedCreatedCountry);

            Assert.Equal(city.City, publishedCreatedCity[0].DisplayName);
            Assert.Equal(mockLocation.Address.Country, publishedCreatedCountry[0].DisplayName);
        }

    }
}
