using AutoMapper;
using Cities.Application.Mappings;
using Cities.Infrastructure.Repositories;
using Cities.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Cities.Core.Features.Cities;

namespace Cities.Tests
{
    public class CityRepositoryIntegrationTests : IClassFixture<PostgresFixture>, IAsyncLifetime
    {
        private readonly PostgresFixture _fixture;
        private ICityRepository _repo;
        private IMapper _mapper;


        public CityRepositoryIntegrationTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            var loggerFactory = new LoggerFactory();
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), loggerFactory));
            var logger = loggerFactory.CreateLogger<CityRepository>();
            _repo = new CityRepository(_fixture.Context, _mapper);
        }

        public Task DisposeAsync() => Task.CompletedTask;


        [Fact]
        public async Task CheckNotRegisteredByAssociatedNames_Returns_Missing_CityNames()
        {
            // Arrange
            List<City> mockCityList = new List<City>()
        {
            new City { DisplayName = "Tokyo", AssociatedNames = ["tokyo"] },
            new City { DisplayName = "New York", AssociatedNames = ["new york"] },
            new City { DisplayName = "Kobenhavn", AssociatedNames = ["kobenhavn"] }
        };
            await _fixture.Context.City.AddRangeAsync(mockCityList);
            await _fixture.Context.SaveChangesAsync();
            List<List<string>> existingCityNames = new List<List<string>> { new List<string> { "tokyo" }, new List<string> { "new york" }, new List<string> { "kobenhavn" } };
            List<List<string>> missingCityNames = new List<List<string>> { new List<string> { "zurick" }, new List<string> { "roma" } };
            List<List<string>> combined = existingCityNames.Concat(missingCityNames).ToList();

            // Act
            List<string> cityList = await _repo.CheckNotRegisteredByAssociatedNames(combined);

            // Assert
            cityList.Should().HaveCount(2);
            cityList.Any(x => missingCityNames[0].Contains(x)).Should().BeTrue();
            cityList.Any(x => missingCityNames[1].Contains(x)).Should().BeTrue();
        }
    }
}
