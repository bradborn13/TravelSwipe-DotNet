
using AutoMapper;
using Countries.Core.Features.Countries;
using TravelSwipe.Shared.Contracts;

namespace Countries.Application.Mappings

{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {

            CreateMap<Country, CountryDto>();
            CreateMap<CountryRegisteredEvent, Country>();
        }
    }
}
