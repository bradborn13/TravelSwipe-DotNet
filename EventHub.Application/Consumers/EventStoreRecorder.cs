using System.Text.Json;
using System.Text.Json.Nodes;
using EventStore.Client;
using Microsoft.Extensions.Logging;
using TravelSwipe.Shared.Messaging;

namespace EventHub.Application.Consumers;

/// <summary>
/// Receives a copy of every message on the exchange (queue bound to "#") and appends it to
/// EventStore, one stream per event type (e.g. "CityRegisteredEvent"). Adding a new event to
/// <see cref="MessagingTopology"/> records it automatically.
/// </summary>
public class EventStoreRecorder : IMessageHandler<JsonNode>
{
    private readonly ILogger<EventStoreRecorder> _logger;
    private readonly EventStoreClient _eventStore;
    private readonly MessageContext _context;

    public EventStoreRecorder(ILogger<EventStoreRecorder> logger, EventStoreClient eventStore, MessageContext context)
    {
        _logger = logger;
        _eventStore = eventStore;
        _context = context;
    }

    public async Task HandleAsync(JsonNode message, CancellationToken cancellationToken)
    {
        // Publishers set the contract name as the AMQP type; fall back to the routing key.
        var eventType = string.IsNullOrWhiteSpace(_context.MessageType) ? _context.RoutingKey : _context.MessageType;

        // Reusing the RabbitMQ message id makes redelivered messages idempotent in EventStore.
        var eventId = Guid.TryParse(_context.MessageId, out var id) ? Uuid.FromGuid(id) : Uuid.NewUuid();

        var metadata = JsonSerializer.SerializeToUtf8Bytes(new
        {
            _context.RoutingKey,
            _context.MessageId,
            _context.PublishedAt,
            RecordedAt = DateTimeOffset.UtcNow,
        });

        var eventData = new EventData(
            eventId,
            eventType,
            JsonSerializer.SerializeToUtf8Bytes(message),
            metadata);

        await _eventStore.AppendToStreamAsync(
            eventType,
            StreamState.Any,
            new[] { eventData },
            cancellationToken: cancellationToken);

        _logger.LogInformation("EventHub - Appended {EventType} ({RoutingKey}) to EventStore.", eventType, _context.RoutingKey);
    }
}
