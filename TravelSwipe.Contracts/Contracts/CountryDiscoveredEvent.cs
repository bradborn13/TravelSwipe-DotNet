using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Contracts.Contracts
{
    public class CountryDiscoveredEvent
    {
        public string CountryCode { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public List<string> AssociatedNames { get; init; }
        public List<string>? AssociatedSlugs { get; set; }
        public DateTime DiscoveredAt { get; init; } = DateTime.UtcNow;



    }
}
