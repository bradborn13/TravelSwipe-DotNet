using AutoMapper;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Models;

namespace EventHub.Application.Mappings
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<CategoriesMQ, CategoryDto>();
            CreateMap<DetailsMQ, DetailsDto>();
            CreateMap<SocialMediaMQ, SocialMediaDto>();
            CreateMap<RelatedPlacesMQ, RelatedPlacesDto>();
            CreateMap<ImageURLMQ, ImageURLDto>();
            CreateMap<ActivityMQ, ActivityDto>();



        }
    }
}
