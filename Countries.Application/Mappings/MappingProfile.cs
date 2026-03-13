
using AutoMapper;
using Countries.Core.Features.Countries;
using TravelSwipe.Contracts.Contracts;

namespace Countries.Application.Mappings

{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {

            CreateMap<Country, CountryDto>();
            CreateMap<CountryDiscoveredEvent, Country>();
        }
    }
}
