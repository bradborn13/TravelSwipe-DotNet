using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Features.Activities;

namespace TravelSwipe.Core.Features.Cities
{
    public class CityService : ICityService
    {
        private readonly ICityRepository _repository;
        private readonly IMapper _mapper;

        public async Task<IEnumerable<CityDto>> getAll()
        {
            var cityList = await _repository.getAll();
            return (IEnumerable<CityDto>)_mapper.Map<CityDto>(cityList);
        }
    }
}
