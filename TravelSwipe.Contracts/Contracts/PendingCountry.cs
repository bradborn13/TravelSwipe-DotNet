namespace TravelSwipe.Shared.Contracts
{
    public class PendingCountryEvent
    {
        public string Name { get; init; }
        public string SlugName { get; set; }
        public DateTime DiscoveredAt { get; init; } = DateTime.UtcNow;

    }
}