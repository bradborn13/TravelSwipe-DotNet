using AutoMapper;
using Cities.Application.Mappings;
using Cities.Core.Features.Countries;
using Cities.Infrastructure.Data;
using Cities.Infrastructure.Repositories;
using Cities.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cities.Tests
{
    public class CountryRepositoryTests
    {
        private CityDbContext _context;
        private ICountryRepository _repo;
        private IMapper mapper;


        public CountryRepositoryTests()
        {
            var fixture = new InMemoryFixture();
            _context = fixture.CreateContext();
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<CountryRepository>();
            mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), loggerFactory));
            _repo = new CountryRepository(_context, mapper);
        }

        [Fact]
        public async Task AddBatch_Should_StoreNewCities()
        {
            Country country = new Country { DisplayName = "Test" };
            // Arrange
            List<Country> countrList = new List<Country>()
            {
              country
            };

            // Act
            await _repo.AddBatch(countrList);
            var saved = _context.Country.ToList();

            // Assert
            saved.Should().HaveCount(1);
            saved.Any(x => x.DisplayName == country.DisplayName).Should().BeTrue();
        }




        [Fact]
        public async Task GetAll_Returns_Cities()
        {
            // Arrange
            List<Country> mockCountryList = new List<Country>()
        {
            new Country { DisplayName = "Tokyo"},
            new Country { DisplayName = "New York"},
            new Country { DisplayName = "Kobenhavn"}
        };
            await _context.Country.AddRangeAsync(mockCountryList);
            await _context.SaveChangesAsync();

            // Act

            List<Country> allCities = await _repo.GetAll();

            // Assert
            allCities.Should().HaveCount(3);
            allCities.Any(x => x.DisplayName == mockCountryList[0].DisplayName).Should().BeTrue();
            allCities.Any(x => x.DisplayName == mockCountryList[1].DisplayName).Should().BeTrue();
            allCities.Any(x => x.DisplayName == mockCountryList[2].DisplayName).Should().BeTrue();
        }
    }
}
