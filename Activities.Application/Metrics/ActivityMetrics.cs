using Prometheus;

namespace Activities.Api.Metrics
{
    public sealed class ActivityMetrics
    {
        public readonly Counter GetActivitiesViaDb = Prometheus.Metrics.CreateCounter("activities_retrieved_total", "Amount of times activities retrieved via db");
        public readonly Counter GetActivitiesViaReddis = Prometheus.Metrics.CreateCounter("activities_getFrom_reddis_total", "Amount of times activities returned from reddis");

        public readonly Counter ActivitiesImageUpdated = Prometheus.Metrics
            .CreateCounter("activities_image_updated_total", "Total images fetched for activities ");

        public readonly Histogram ActivityScraptingEventsDuration = Prometheus.Metrics
            .CreateHistogram("activity_scraping_events_duration_seconds", "Duration of activity scraping events for new location");

        public readonly Histogram ActivityScraptingPhotosDuration = Prometheus.Metrics
           .CreateHistogram("activity_scraping_photos_duration_seconds", "Duration of activity scraping photos for new location");

        //public readonly Gauge ActivePublishOperations = Prometheus.Metrics
        //    .CreateGauge("activities_rabbitmq_publish_active", "RabbitMQ publish operations in flight");
    }
}
