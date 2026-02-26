using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Features.Activities;
using TravelSwipe.Core.Features.Cities;
using TravelSwipe.Core.Features.Users;

namespace TravelSwipe.Infrastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<City, CityDto>();
            CreateMap<Categories, CategoryDto>();

            CreateMap<Details, DetailsDto>();

            CreateMap<SocialMedia, SocialMediaDto>();

            CreateMap<RelatedPlaces, RelatedPlacesDto>()
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));


            CreateMap<Activity, ActivityDto>();
        }
    }
}
