using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TravelSwipe.Core.Features.Cities;
using TravelSwipe.Infrastructure.data;

namespace TravelSwipe.Infrastructure.Repositories
{

    public class CityRepository : ICityRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CityRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<List<string>> GetCityNames()
        {
            var cityList = await _context.City.Select(x => x.NameClean).ToListAsync();

            return _mapper.Map<List<string>>(cityList ?? []);
        }
        public async Task<List<City>> GetAll()
        {
            return await _context.City.ToListAsync();
        }
        public async Task<List<string>> FindCitiesNotRegisterd(List<string> cityList)
        {
            var existingCities = (await _context.City
                .Select(x => x.NameClean)
                .ToListAsync())
                .ToHashSet();

            var missingCities = cityList
                .Where(city => !existingCities.Contains(city))
                .ToList();
            return missingCities;
        }
        public async Task AddBatch(List<City> cityList)
        {
            await _context.City.AddRangeAsync(cityList);
            await _context.SaveChangesAsync();
        }

    }
}
