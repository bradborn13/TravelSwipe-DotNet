using AutoMapper;
using Cities.Core.Features.Cities;

namespace Cities.Application.Mappings

{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<City, CityDto>();

        }
    }
}
