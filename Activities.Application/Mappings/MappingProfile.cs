using AutoMapper;
using TravelSwipe.Activities.Core.Features.Activities;

namespace TravelSwipe.Infrastructure.Mappings
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            //CreateMap<User, UserDto>();
            //CreateMap<City, CityDto>();
            //CreateMap<City, NomResponse>();
            CreateMap<Categories, CategoryDto>();
            CreateMap<FourSquareActivities, Activity>();
            CreateMap<Details, DetailsDto>();
            CreateMap<DetailsDto, Details>();
            CreateMap<SocialMedia, SocialMediaDto>();
            CreateMap<SocialMediaDto, SocialMedia>();
            CreateMap<RelatedPlaces, RelatedPlacesDto>();
            CreateMap<RelatedPlacesDto, RelatedPlaces>()
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));
            CreateMap<ImageURLDto, ImageURL>();
            CreateMap<ImageURL, ImageURLDto>();
            CreateMap<Activity, ActivityDto>();
            CreateMap<FSCategory, Categories>();
            CreateMap<FSLocation, Details>();
            CreateMap<FSActivity, Activity>()
          .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
          .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
          .ForMember(dest => dest.Distance, opt => opt.MapFrom(src => src.Distance))
          .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src.Link))
          .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories ?? new List<FSCategory>()))
.ForMember(dest => dest.RelatedPlaces,
    opt => opt.MapFrom(src => src.RelatedPlaces == null ? null : src.RelatedPlaces.Children))
.ForMember(dest => dest.ImagesURL, opt => opt.MapFrom(src => new List<ImageURL>()))
          //.ForMember(dest => dest.Details, opt => opt.MapFrom(src  new Details()))
          .ForMember(dest => dest.DateCreated,
           opt => opt.MapFrom(src => string.IsNullOrEmpty(src.DateCreated) ? (DateTime?)null : DateTime.Parse(src.DateCreated)))
.ForMember(dest => dest.DateRefreshed,
           opt => opt.MapFrom(src => string.IsNullOrEmpty(src.DateRefreshed) ? (DateTime?)null : DateTime.Parse(src.DateRefreshed)))
          .ForMember(dest => dest.SocialMedia, opt => opt.MapFrom(src => new SocialMedia()));
            CreateMap<FSChildPlace, RelatedPlaces>()
    .ForMember(d => d.Fsq_place_id,
        o => o.MapFrom(s => s.FsqPlaceId));



        }
    }
}
