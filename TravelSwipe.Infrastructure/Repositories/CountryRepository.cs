using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

using TravelSwipe.Core.Features.Countries;
using TravelSwipe.Infrastructure.data;

namespace TravelSwipe.Infrastructure.Repositories
{

    public class CountryRepository : ICountryRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public CountryRepository(AppDbContext context, IMapper mapper)
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
