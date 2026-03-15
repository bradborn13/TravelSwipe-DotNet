namespace TravelSwipe.Contracts.Contracts
{
    public class PendingCityEvent
    {
        public List<string> Name { get; init; }
        public List<string>? SlugName { get; set; }
        public DateTime DiscoveredAt { get; init; } = DateTime.UtcNow;
    }
}