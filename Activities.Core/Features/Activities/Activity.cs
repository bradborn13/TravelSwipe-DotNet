using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Activities.Core.Features.Activities
{
    public class SocialMedia
    {
        public string FacebookId { get; set; } = string.Empty;
        public string Instagram { get; set; } = string.Empty;
        public string Twitter { get; set; } = string.Empty;

    }
    public class Categories
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;

    }
    public class Details
    {
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        //public string Cross_street { get; set; } = string.Empty;
        public string FormattedAddress { get; set; } = string.Empty;
        public string Locality { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

    }

    public class RelatedPlaces
    {
        public string? Fsq_place_id { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
        public Categories[]? Categories { get; set; }

    }
    public class ImageURL
    {
        public double? OriginalHeight { get; set; }
        public double? OriginalWidth { get; set; }
        public string? Thumbnail { get; set; }
        public string? Title { get; set; }
        public string? Source { get; set; }
        public string? Link { get; set; }
        public int? Position { get; set; }
        public string? ImgSource { get; set; }
    }
    public class Activity
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public required string City { get; set; }
        public string? Country { get; set; }
        public string? Suburb { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public required double Latitude { get; set; }
        public required double Longitude { get; set; }

        public string FsqId { get; set; } = string.Empty;
        public List<ImageURL>? ImagesURL { get; set; }

        public List<Categories>? Categories { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateRefreshed { get; set; }
        public string? Link { get; set; }
        public string? Tel { get; set; }
        public Details? Details { get; set; }
        public int? Distance { get; set; }
        public List<RelatedPlaces>? RelatedPlaces { get; set; }
        public SocialMedia? SocialMedia { get; set; }
    }
    public class CityGeoLocation
    {
        public int _id { get; set; }
        public required string City { get; set; }
        public required double Longitude { get; set; }
        public required double Latitude { get; set; }
    }
}
