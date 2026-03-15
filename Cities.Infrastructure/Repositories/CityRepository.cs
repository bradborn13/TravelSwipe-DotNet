using AutoMapper;
using Cities.Core.Features.Cities;
using Cities.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cities.Infrastructure.Repositories
{

    public class CityRepository : ICityRepository
    {
        private readonly CityDbContext _context;
        private readonly IMapper _mapper;

        public CityRepository(CityDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<List<string>> GetCityNames()
        {
            var cityList = await _context.City.Select(x => x.DisplayName).ToListAsync();

            return _mapper.Map<List<string>>(cityList ?? []);
        }
        public async Task<List<City>> GetAll()
        {
            return await _context.City.ToListAsync();
        }
        public async Task<List<string>> FindCitiesNotRegisterd(List<string> cityList)
        {
            var existingCities = (await _context.City
                .Select(x => x.DisplayName)
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
        public async Task<List<string>> CheckNotRegisteredByAssociatedNames(List<List<string>> cityList)
        {
            var flatList = cityList.SelectMany(x => x).ToList();

            var existingNames = await _context.City
                              .Where(c => c.AssociatedNames != null && c.AssociatedNames.Any(n => flatList.Contains(n)))
                              .SelectMany(c => c.AssociatedNames )
                              .ToListAsync();

            var nonExistingNames = flatList.Where(n => !existingNames.Contains(n)).ToList();
            return nonExistingNames;
        }


    }
}
