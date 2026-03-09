using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Core.Users;
using TravelSwipe.Core.Features.Activities;
using TravelSwipe.Core.Features.Cities;

namespace TravelSwipe.Application.Services
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
    }
}
