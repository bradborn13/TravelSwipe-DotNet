using AutoMapper;
using Countries.Core.Features.Countries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Countries.Application.Services
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
        public async Task<List<CountryDto>> GetAll()
        {
            var countryList = await _repository.GetAll();
            return _mapper.Map<List<CountryDto>>(countryList);
        }
        public async Task AddCountries(List<Country> countryList)
        {
            var assciatedNamesList = countryList.Where(x => x.AssociatedNames != null).Select(x => x.AssociatedNames!).ToList();
            var missingCountry = await _repository.CheckNotRegisteredByAssociatedNames(assciatedNamesList);
            if (missingCountry.Count() > 0)
            {
                await _repository.AddBatch(countryList);
            }
            return;

        }

    }
}
