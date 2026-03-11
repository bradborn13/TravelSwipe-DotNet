using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace TravelSwipe.Activities.Core.Features.Activities
{
    public class FourSquareActivities
    {
        [JsonPropertyName("results")]
        public List<FSActivity>? Results { get; set; }
    }

    public class FSActivity
    {
        [JsonPropertyName("fsq_place_id")]
        public required string FsqId { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("distance")]
        public int Distance { get; set; }

        [JsonPropertyName("date_created")]
        public required string DateCreated { get; set; }

        [JsonPropertyName("date_refreshed")]
        public required string DateRefreshed { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("placemaker_url")]
        public string? PlacemakerUrl { get; set; }

        //[JsonPropertyName("extended_location")]
        //public object? ExtendedLocation { get; set; }

        [JsonPropertyName("categories")]
        public List<FSCategory>? Categories { get; set; }

        [JsonPropertyName("location")]
        public FSLocation? Details { get; set; }

        [JsonPropertyName("related_places")]
        public FSRelatedPlaces? RelatedPlaces { get; set; }
    }

    public class FSCategory
    {
        [JsonPropertyName("fsq_category_id")]
        public required string Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("short_name")]
        public string? ShortName { get; set; }

        [JsonPropertyName("plural_name")]
        public string? PluralName { get; set; }

        //[JsonPropertyName("icon")]
        //public FSIcon? Icon { get; set; }
    }

    public class FSIcon
    {
        [JsonPropertyName("prefix")]
        public string? Prefix { get; set; }

        [JsonPropertyName("suffix")]
        public string? Suffix { get; set; }
    }

    public class FSLocation
    {
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("locality")]
        public string? Locality { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("formatted_address")]
        public string? FormattedAddress { get; set; }
    }


    public class FSRelatedPlaces
    {
        [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
        public FSChildPlace Parent { get; set; }

        [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
        public List<FSChildPlace> Children { get; set; }
    }
    public partial class FSChildPlace
    {
        [JsonProperty("fsq_place_id")]
        public string FsqPlaceId { get; set; }

        [JsonProperty("categories")]
        public List<FSCategory> Categories { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

}
