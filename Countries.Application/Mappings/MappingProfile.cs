
using AutoMapper;
using Countries.Core.Features.Countries;

namespace Countries.Application.Mappings

{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {

            CreateMap<Country, CountryDto>();
        }
    }
}
