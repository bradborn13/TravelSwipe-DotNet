using AutoMapper;
using Countries.Core.Features.Countries;
using Countries.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Countries.Infrastructure.Repositories
{

    public class CountryRepository : ICountryRepository
    {
        private readonly CountryDbContext _context;
        private readonly IMapper _mapper;
        public CountryRepository(CountryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<Country>> GetAll()
        {
            var countries = await _context.Country.ToListAsync();

            return countries;
        }
        public async Task<List<string>> CheckNotRegisteredByAssociatedNames(List<List<string>> countryList)
        {
            var flatList = countryList.SelectMany(x => x).ToList();

            var existingNames = await _context.Country
                              .Where(c => c.AssociatedNames != null && c.AssociatedNames.Any(n => flatList.Contains(n)))
                              .SelectMany(c => c.AssociatedNames ?? new List<string>())
                              .ToListAsync();

            var nonExistingNames = flatList.Where(n => !existingNames.Contains(n)).ToList();
            return nonExistingNames;
        }
        public async Task AddBatch(List<Country> countryList)
        {
            await _context.Country.AddRangeAsync(countryList);
            await _context.SaveChangesAsync();
        }

    }
}
