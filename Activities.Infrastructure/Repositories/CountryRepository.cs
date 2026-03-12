using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Activities.Core.Features.Cities;
using TravelSwipe.Activities.Core.Features.Countries;
using TravelSwipe.Infrastructure.data;

namespace TravelSwipe.Infrastructure.Repositories
{

    public class CountryRepository : ICountryRepository
    {
        private readonly ActivityDbContext _context;
        private readonly IMapper _mapper;
        public CountryRepository(ActivityDbContext context, IMapper mapper)
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
