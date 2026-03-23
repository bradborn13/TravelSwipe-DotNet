using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Activities.Core.Features.Activities
{
    public record ActivityDto(
        string Id,
        string Name,
        string? City,
        string? Suburb,
        string? Address,
        string? Website,
        double? Latitude,
        double? Longitude,
        // Flattened data for easier UI binding
        List<CategoryDto> Categories,
        string? Link,
        string? Tel,
        DetailsDto Details,
        double Distance,
        DateTime DateCreated,
        DateTime DateRefreshed,
        List<RelatedPlacesDto> RelatedPlaces,
        SocialMediaDto SocialMedia,
        List<ImageURLDto> ImagesURL
    );
    public record DetailsDto(string FormattedAddress, string Postcode, string Region);
    public record SocialMediaDto(string Instagram, string FacebookId);
    public record CategoryDto(string Name, string ShortName);
    public record RelatedPlacesDto(string Name, List<CategoryDto> Categories);
    public record ImageURLDto(double? OriginalHeight,
        double? OriginalWidth,
        string Thumbnail,
        string Title,
        string Source,
        string Link,
        int Position,
        string ImgSource);

}
