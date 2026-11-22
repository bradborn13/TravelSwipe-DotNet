using Integration.Core.Features.Activities;
using TravelSwipe.Shared.Messaging;
using Microsoft.Extensions.Logging;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Models;

namespace Integration.Application.Consumer
{
    public class FetchLatestImagesForLocationConsumer : IMessageHandler<FetchLatestImagesForLocation>
    {
        private readonly ILogger<FetchLatestImagesForLocationConsumer> _logger;
        private readonly ITavilyService _tavilyService;
        private readonly IEventPublisher _publisher;
        public FetchLatestImagesForLocationConsumer(
       ILogger<FetchLatestImagesForLocationConsumer> logger, ITavilyService tavilyService, IEventPublisher publisher)
        {
            _logger = logger;
            _tavilyService = tavilyService;
            _publisher = publisher;
        }
        public async Task HandleAsync(FetchLatestImagesForLocation @event, CancellationToken cancellationToken)
        {
            
            try
            {
                _logger.LogInformation("FetchLatestImages for city {city}", @event.City);

                var tasks = @event.Activities
                          .Select(async activity => new
                          {
                              Activity = activity,
                              Images = await _tavilyService.GetImages(activity, @event.City)
                          })
                          .ToList();
                var results = await Task.WhenAll(tasks);
                var activityImagePackage = results.ToDictionary(x => x.Activity, x => x.Images.Select(img => new ImageURLMQ() { Source = "Tavily", Thumbnail = img }).ToList());


                await _publisher.PublishAsync(new LatestImagesForActivitiesEvent
                {
                    City = @event.City,
                    ImagePackageByActivity = activityImagePackage
                }, cancellationToken);

                _logger.LogInformation("FetchLatestImages processed for location {city} , returning new images ", @event.City);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in FetchLatestImages for location {City}, exception:{exception}", @event.City, ex.Message);
                throw;
            }
        }
    }
}
