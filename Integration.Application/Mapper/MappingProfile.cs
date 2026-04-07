using AutoMapper;
using Integration.Application.Integrations.Images;
using Integration.Core.Features.Activities;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Models;

namespace Infrastructure.Mappings
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<FSCategory, CategoriesMQ>();
            CreateMap<FSLocation, DetailsMQ>();
            CreateMap<FSActivity, ActivityMQ>()
          .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
          .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
          .ForMember(dest => dest.Distance, opt => opt.MapFrom(src => src.Distance))
          .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src.Link))
          .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories ?? new List<FSCategory>()))
.ForMember(dest => dest.RelatedPlaces,
    opt => opt.MapFrom(src => src.RelatedPlaces == null ? null : src.RelatedPlaces.Children))
.ForMember(dest => dest.ImagesURL, opt => opt.MapFrom(src => new List<ImageURLMQ>()))
          .ForMember(dest => dest.DateCreated,
           opt => opt.MapFrom(src => string.IsNullOrEmpty(src.DateCreated) ? (DateTime?)null : DateTime.Parse(src.DateCreated)))
.ForMember(dest => dest.DateRefreshed,
           opt => opt.MapFrom(src => string.IsNullOrEmpty(src.DateRefreshed) ? (DateTime?)null : DateTime.Parse(src.DateRefreshed)))
          .ForMember(dest => dest.SocialMedia, opt => opt.MapFrom(src => new SocialMediaMQ()));
            CreateMap<FSChildPlace, RelatedPlacesMQ>()
    .ForMember(d => d.Fsq_place_id,
        o => o.MapFrom(s => s.FsqPlaceId));

            CreateMap<ActivityMQ, ActivityMQ>();
            CreateMap<ImageURL, ImageURLMQ>();



        }

    }
}
