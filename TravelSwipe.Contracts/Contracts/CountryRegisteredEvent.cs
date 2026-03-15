using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Contracts.Contracts
{
    public record CountryRegisteredEvent
    {
        public required string CountryCode { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public required string SlugName { get; set; }
        public DateTime DiscoveredAt { get; init; } = DateTime.UtcNow;



    }
}
