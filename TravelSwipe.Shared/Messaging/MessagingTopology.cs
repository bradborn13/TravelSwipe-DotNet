using RabbitMQ.Client;
using TravelSwipe.Shared.Contracts;

namespace TravelSwipe.Shared.Messaging
{
    /// <summary>
    /// Single source of truth for the RabbitMQ topology shared by every service.
    /// Every service declares the full topology on startup (idempotent), so a message
    /// published before its consumer service has started is still queued, not dropped.
    /// </summary>
    public static class MessagingTopology
    {
        public const string Exchange = "travelswipe-exchange";

        /// <summary>Suffix for the queue that receives messages whose handler threw.</summary>
        public const string ErrorQueueSuffix = "_error";

        public static class RoutingKeys
        {
            public const string CityRegistered = "city.registered";
            public const string CountryRegistered = "country.registered";
            public const string ScrapeLocationActivities = "activities.scrape.requested";
            public const string FoundActivitiesForLocation = "activities.found";
            public const string ActivitiesByLocation = "activities.by.location";
            public const string FetchLatestImagesForLocation = "images.fetch.requested";
            public const string LatestImagesForActivities = "images.updated";
            /// <summary>Topic wildcard: matches every routing key.</summary>
            public const string All = "#";
        }

        public static class Queues
        {
            // Cities.Api
            public const string CityService = "city-service-queue";
            // Countries.Api
            public const string CountryService = "country-service-queue";
            // Activities.Api
            public const string ActivityFound = "activity-found-queue";
            public const string ActivityImages = "activity-images-queue";
            // Integration.Worker
            public const string ActivityScrape = "activity-scrape-queue";
            public const string IntegrationImages = "integration-images-queue";
            // EventHub.Worker
            public const string EventHubActivities = "eventhub-activities-queue";
            public const string EventHubImages = "eventhub-images-queue";
            /// <summary>Receives a copy of every message on the exchange, to append it to EventStore.</summary>
            public const string EventHubEventStore = "eventhub-eventstore-queue";
        }

        private static readonly Dictionary<Type, string> RoutingKeyByMessage = new()
        {
            [typeof(CityRegisteredEvent)] = RoutingKeys.CityRegistered,
            [typeof(CountryRegisteredEvent)] = RoutingKeys.CountryRegistered,
            [typeof(ScrapeLocationActivitiesEvent)] = RoutingKeys.ScrapeLocationActivities,
            [typeof(FoundActivitiesForLocationEvent)] = RoutingKeys.FoundActivitiesForLocation,
            [typeof(ActivitiesByLocationEvent)] = RoutingKeys.ActivitiesByLocation,
            [typeof(FetchLatestImagesForLocation)] = RoutingKeys.FetchLatestImagesForLocation,
            [typeof(LatestImagesForActivitiesEvent)] = RoutingKeys.LatestImagesForActivities,
        };

        private static readonly (string Queue, string RoutingKey)[] Bindings =
        {
            (Queues.CityService, RoutingKeys.CityRegistered),
            (Queues.CountryService, RoutingKeys.CountryRegistered),
            (Queues.ActivityScrape, RoutingKeys.ScrapeLocationActivities),
            (Queues.ActivityFound, RoutingKeys.FoundActivitiesForLocation),
            (Queues.EventHubActivities, RoutingKeys.ActivitiesByLocation),
            (Queues.IntegrationImages, RoutingKeys.FetchLatestImagesForLocation),
            (Queues.ActivityImages, RoutingKeys.LatestImagesForActivities),
            (Queues.EventHubImages, RoutingKeys.LatestImagesForActivities),
            (Queues.EventHubEventStore, RoutingKeys.All),
        };

        public static string RoutingKeyFor<T>() =>
            RoutingKeyByMessage.TryGetValue(typeof(T), out var key)
                ? key
                : throw new InvalidOperationException(
                    $"No routing key registered for message type {typeof(T).Name}. Add it to {nameof(MessagingTopology)}.");

        public static async Task DeclareAsync(IChannel channel, CancellationToken cancellationToken = default)
        {
            await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true, autoDelete: false,
                cancellationToken: cancellationToken);

            foreach (var queue in Bindings.Select(b => b.Queue).Distinct())
            {
                await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false,
                    cancellationToken: cancellationToken);
                await channel.QueueDeclareAsync(queue + ErrorQueueSuffix, durable: true, exclusive: false, autoDelete: false,
                    cancellationToken: cancellationToken);
            }

            foreach (var (queue, routingKey) in Bindings)
            {
                await channel.QueueBindAsync(queue, Exchange, routingKey, cancellationToken: cancellationToken);
            }
        }
    }
}
