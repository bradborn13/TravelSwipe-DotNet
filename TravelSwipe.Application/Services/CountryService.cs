using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Features.Cities;
using TravelSwipe.Core.Features.Countries;

namespace TravelSwipe.Application.Services
{
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository _repository;
        private readonly IMapper _mapper;

        public CountryService(ICountryRepository countryRepository, IMapper mapper)
        {
            _repository = countryRepository;
            _mapper = mapper;
        }
        public async Task<List<CountryDto>> getAll()
        {
            var cityList = await _repository.GetAll();
            return _mapper.Map<List<CountryDto>>(cityList);
        }
    }
}
