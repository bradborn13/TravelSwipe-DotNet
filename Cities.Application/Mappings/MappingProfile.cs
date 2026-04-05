using AutoMapper;
using Cities.Core.Features.Cities;
using TravelSwipe.Shared.Contracts;

namespace Cities.Application.Mappings

{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<City, CityDto>();
            CreateMap<CityRegisteredEvent, City>();
        }
    }
}
