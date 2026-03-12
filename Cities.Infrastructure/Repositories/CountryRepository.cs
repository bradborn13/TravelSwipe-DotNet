using AutoMapper;
using Cities.Core.Features.Countries;
using Cities.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Cities.Infrastructure.Repositories
{

    public class CountryRepository : ICountryRepository
    {
        private readonly CityDbContext _context;
        private readonly IMapper _mapper;
        public CountryRepository(CityDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<Country>> GetAll()
        {
            var countries = await _context.Country.ToListAsync();

            return countries;
        }
        public async Task AddBatch(List<Country> countryList)
        {
            await _context.Country.AddRangeAsync(countryList);
            await _context.SaveChangesAsync();


        }

    }
}
