using AutoMapper;
using Cities.Core.Features.Cities;
using TravelSwipe.Contracts.Contracts;

namespace Cities.Application.Mappings

{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<City, CityDto>();
            CreateMap<CityDiscoveredEvent, City>();
        }
    }
}
