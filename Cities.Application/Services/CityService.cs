using AutoMapper;
using Cities.Core.Features.Cities;



namespace Cities.Application.Services
{
    public class CityService : ICityService
    {
        private readonly ICityRepository _repository;
        private readonly IMapper _mapper;

        public CityService(ICityRepository cityRepository, IMapper mapper)
        {
            _repository = cityRepository;
            _mapper = mapper;
        }
        public async Task<List<string>> GetAll()
        {
            var cityList = await _repository.GetCityNames() ?? [];
            return _mapper.Map<List<string>>(cityList);
        }
        public async Task AddCities(List<City> cityList)
        {
            await _repository.AddBatch(cityList);
        }
    }
}
