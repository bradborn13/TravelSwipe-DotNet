using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Shared.Models
{
    public class SocialMediaMQ
    {
        public string FacebookId { get; set; } = string.Empty;
        public string Instagram { get; set; } = string.Empty;
        public string Twitter { get; set; } = string.Empty;

    }
    public class CategoriesMQ
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;

    }
    public class DetailsMQ
    {
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        //public string Cross_street { get; set; } = string.Empty;
        public string FormattedAddress { get; set; } = string.Empty;
        public string Locality { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

    }

    public class RelatedPlacesMQ
    {
        public string? Fsq_place_id { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
        public CategoriesMQ[]? Categories { get; set; }

    }
    public class ImageURLMQ
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
    public class ActivityMQ
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
        public List<ImageURLMQ>? ImagesURL { get; set; }

        public List<CategoriesMQ>? Categories { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateRefreshed { get; set; }
        public string? Link { get; set; }
        public string? Tel { get; set; }
        public DetailsMQ? Details { get; set; }
        public int? Distance { get; set; }
        public List<RelatedPlacesMQ>? RelatedPlaces { get; set; }
        public SocialMediaMQ? SocialMedia { get; set; }
    }
    public class CityGeoLocationMQ
    {
        public int _id { get; set; }
        public required string City { get; set; }
        public required double Longitude { get; set; }
        public required double Latitude { get; set; }
    }
}
