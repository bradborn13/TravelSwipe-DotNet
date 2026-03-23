using AutoMapper;
using Contries.Tests.Fixtures;
using Countries.Application.Mappings;
using Countries.Core.Features.Countries;
using Countries.Infrastructure.Data;
using Countries.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Contries.Tests;

public class CountryRepositoryTests
{
    private CountryDbContext _context;
    private ICountryRepository _repository;
    private IMapper mapper;

    public CountryRepositoryTests()
    {
        var fixture = new InMemoryFixture();
        _context = fixture.CreateContext();
        var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<CountryRepository>();
        mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), loggerFactory));
        _repository = new CountryRepository(_context, mapper);
    }
    [Fact]
    public async Task AddBatch_Should_StoreNewCountries()
    {
        Country addCountry = new Country { DisplayName = "Test", AssociatedNames = ["Test"], };
        // Arrange
        List<Country> cityList = new List<Country>()
        {
          addCountry
        };

        // Act
        await _repository.AddBatch(cityList);
        List<Country> saved = await _context.Country.ToListAsync();

        // Assert
        saved.Should().HaveCount(1);
        saved.Any(x => x.DisplayName == addCountry.DisplayName).Should().BeTrue();
    }

    [Fact]
    public async Task GetAll_Returns_CityList()
    {
        // Arrange
        List<Country> mockCityList = new List<Country>()
        {
           new Country { DisplayName = "Brazil", AssociatedNames = ["brazil"], },
           new Country { DisplayName = "Denmark", AssociatedNames = ["denmark"], },
           new Country { DisplayName = "Cyprus", AssociatedNames = ["cyprus"], }
        };
        await _context.Country.AddRangeAsync(mockCityList);
        await _context.SaveChangesAsync();

        // Act
        List<Country> countryList = await _repository.GetAll();
        List<Country> saved = await _context.Country.ToListAsync();

        // Assert
        saved.Should().HaveCount(3);
    }
}
