using Integration.Core.Features.Activities;
using MassTransit;
using Microsoft.Extensions.Logging;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Models;

namespace Integration.Application.Consumer
{
    public class FetchLatestImagesForLocationConsumer : IConsumer<FetchLatestImagesForLocation>
    {
        private readonly ILogger<FetchLatestImagesForLocationConsumer> _logger;
        private readonly ITavilyService _tavilyService;
        private readonly IPublishEndpoint _publishEndpoint;
        public FetchLatestImagesForLocationConsumer(
       ILogger<FetchLatestImagesForLocationConsumer> logger, ITavilyService tavilyService, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _tavilyService = tavilyService;
            _publishEndpoint = publishEndpoint;
        }
        public async Task Consume(ConsumeContext<FetchLatestImagesForLocation> context)
        {
            var @event = context.Message;

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


                await _publishEndpoint.Publish(new LatestImagesForActivitiesEvent
                {
                    City = @event.City,
                    ImagePackageByActivity = activityImagePackage
                });

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
