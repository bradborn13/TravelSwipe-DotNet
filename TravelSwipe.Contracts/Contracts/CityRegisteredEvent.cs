using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Contracts.Contracts
{
    public record CityRegisteredEvent
    {
        public required string Country { get; init; }
        public string Municipality { get; init; } = string.Empty;
        public required List<string> SlugList { get; set; }
        public required List<string> NameList { get; set; }

        public string? Postcode { get; init; }
        public List<string>? State { get; init; }
        public string? DisplayName { get; init; }
        public DateTime DiscoveredAt { get; init; } = DateTime.UtcNow;
    }

}
