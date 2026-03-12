using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Contracts.Contracts
{
    public record CityDiscoveredEvent
    {
        public string Country { get; init; } = string.Empty;
        public string Municipality { get; init; } = string.Empty;
        public List<string> AssociatedNames { get; init; }
        public List<string>? AssociatedSlugs { get; set; }
        public string? Postcode { get; init; }
        public List<string>? State { get; init; }
        public string? DisplayName { get; init; }
        public DateTime DiscoveredAt { get; init; } = DateTime.UtcNow;
    }

}
