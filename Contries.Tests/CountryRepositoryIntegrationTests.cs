using AutoMapper;
using Contries.Tests.Fixtures;
using Countries.Application.Mappings;
using Countries.Core.Features.Countries;
using Countries.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;


namespace Contries.Tests
{
    public class CountryRepositoryIntegrationTests : IClassFixture<PostgresFixture>, IAsyncLifetime
    {
        private readonly PostgresFixture _fixture;
        private ICountryRepository _repo;
        private IMapper _mapper;


        public CountryRepositoryIntegrationTests(PostgresFixture fixture, IMapper mapper, ICountryRepository repo)
        {
            _fixture = fixture;
            _mapper = mapper;
            _repo = repo;
        }
        public async Task InitializeAsync()
        {
            var loggerFactory = new LoggerFactory();
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), loggerFactory));
            var logger = loggerFactory.CreateLogger<CountryRepository>();
            _repo = new CountryRepository(_fixture.Context, _mapper);
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task CheckNotRegisteredByAssociatedNames_Returns_Missing_CityNames()
        {
            // Arrange
            List<Country> mockCountryList = new List<Country>()
        {
           new Country { DisplayName = "Brazil", AssociatedNames = ["brazil"], },
        };
            await _fixture.Context.Country.AddRangeAsync(mockCountryList);
            await _fixture.Context.SaveChangesAsync();
            List<List<string>> existingCountryNames = new List<List<string>> { new List<string> { "brazil" } };
            List<List<string>> missingCountryNames = new List<List<string>> { new List<string> { "denmark" }, new List<string> { "cyprus" } };
            List<List<string>> combined = existingCountryNames.Concat(missingCountryNames).ToList();

            // Act
            List<string> countryList = await _repo.CheckNotRegisteredByAssociatedNames(combined);

            // Assert
            countryList.Should().HaveCount(2);
            countryList.Any(x => missingCountryNames[0].Contains(x)).Should().BeTrue();
            countryList.Any(x => missingCountryNames[1].Contains(x)).Should().BeTrue();
        }

    }
}
