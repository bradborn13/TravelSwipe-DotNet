using AutoMapper;
using Cities.Application.Mappings;
using Cities.Core.Features.Cities;
using Cities.Infrastructure.Data;
using Cities.Infrastructure.Repositories;
using Cities.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Cities.Tests
{
    public class CityRepositoryTests
    {
        private CityDbContext _context;
        private ICityRepository _repo;
        private IMapper mapper;


        public CityRepositoryTests()
        {
            var fixture = new InMemoryFixture();
            _context = fixture.CreateContext();
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<CityRepository>();
            mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), loggerFactory));
            _repo = new CityRepository(_context, mapper);
        }

        [Fact]
        public async Task AddBatch_Should_StoreNewCities()
        {
            City addCity = new City { DisplayName = "Test", AssociatedNames = ["Test"] };
            // Arrange
            List<City> cityList = new List<City>()
        {
          addCity
        };

            // Act
            await _repo.AddBatch(cityList);
            var saved = _context.City.ToList();

            // Assert
            saved.Should().HaveCount(1);
            saved.Any(x => x.DisplayName == addCity.DisplayName).Should().BeTrue();
        }

        [Fact]
        public async Task GetCityNames_Returns_CityNameList()
        {
            // Arrange
            List<City> mockCityList = new List<City>()
        {
            new City { DisplayName = "Tokyo", AssociatedNames = ["tokyo"] },
            new City { DisplayName = "New York", AssociatedNames = ["new york"] },
            new City { DisplayName = "Kobenhavn", AssociatedNames = ["kobenhavn"] }
        };
            await _context.City.AddRangeAsync(mockCityList);
            await _context.SaveChangesAsync();
            // Act
            List<string> cityList = await _repo.GetCityNames();

            // Assert
            cityList.Should().HaveCount(3);
            cityList.Any(x => x == mockCityList[0].DisplayName).Should().BeTrue();
            cityList.Any(x => x == mockCityList[1].DisplayName).Should().BeTrue();
            cityList.Any(x => x == mockCityList[2].DisplayName).Should().BeTrue();
        }


        [Fact]
        public async Task FindCitiesNotRegisterd_Returns_Missing_CityNames()
        {
            // Arrange
            List<City> mockCityList = new List<City>()
        {
            new City { DisplayName = "Tokyo", AssociatedNames = ["tokyo"] },
            new City { DisplayName = "New York", AssociatedNames = ["new york"] },
            new City { DisplayName = "Kobenhavn", AssociatedNames = ["kobenhavn"] }
        };
            await _context.City.AddRangeAsync(mockCityList);
            await _context.SaveChangesAsync();

            List<string> existingCityNames = new List<String> { "Tokyo", "New York", "Kobenhavn" };
            List<string> missingCityNames = new List<String> { "Zurick", "Roma" };
            // Act

            List<string> repoMissingCities = await _repo.FindCitiesNotRegisterd(existingCityNames.Concat(missingCityNames).ToList());

            // Assert
            repoMissingCities.Should().HaveCount(2);
            repoMissingCities.Any(x => x == missingCityNames[0]).Should().BeTrue();
            repoMissingCities.Any(x => x == missingCityNames[1]).Should().BeTrue();
            repoMissingCities.Any(x => existingCityNames.Contains(x)).Should().BeFalse();
        }
        [Fact]
        public async Task GetAll_Returns_Cities()
        {
            // Arrange
            List<City> mockCityList = new List<City>()
        {
            new City { DisplayName = "Tokyo", AssociatedNames = ["tokyo"] },
            new City { DisplayName = "New York", AssociatedNames = ["new york"] },
            new City { DisplayName = "Kobenhavn", AssociatedNames = ["kobenhavn"] }
        };
            await _context.City.AddRangeAsync(mockCityList);
            await _context.SaveChangesAsync();

            // Act

            List<City> allCities = await _repo.GetAll();

            // Assert
            allCities.Should().HaveCount(3);
            allCities.Any(x => x.DisplayName == mockCityList[0].DisplayName).Should().BeTrue();
            allCities.Any(x => x.DisplayName == mockCityList[1].DisplayName).Should().BeTrue();
            allCities.Any(x => x.DisplayName == mockCityList[2].DisplayName).Should().BeTrue();
        }

    }
}
