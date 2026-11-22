namespace TravelSwipe.Shared.Messaging
{
    /// <summary>
    /// Metadata of the message currently being handled. Scoped: inject it into an
    /// <see cref="IMessageHandler{T}"/> to read routing key, type, id, etc.
    /// </summary>
    public sealed class MessageContext
    {
        public string Queue { get; internal set; } = string.Empty;
        public string Exchange { get; internal set; } = string.Empty;
        public string RoutingKey { get; internal set; } = string.Empty;
        /// <summary>Contract type name set by the publisher (e.g. "CityRegisteredEvent").</summary>
        public string? MessageType { get; internal set; }
        public string? MessageId { get; internal set; }
        /// <summary>When the publisher sent the message (UTC), if it set a timestamp.</summary>
        public DateTimeOffset? PublishedAt { get; internal set; }
    }
}
