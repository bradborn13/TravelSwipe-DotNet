using Activities.Application.Services.Activities;
using Activities.Core.Features.Activities;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Models;

namespace Activities.Application.Consumers
{
    public class LatestImagesForActivitiesConsumer : IConsumer<LatestImagesForActivitiesEvent>
    {
        private readonly ILogger<LatestImagesForActivitiesConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ActivityService _activityService;
        public LatestImagesForActivitiesConsumer(
       ILogger<LatestImagesForActivitiesConsumer> logger, IPublishEndpoint publishEndpoint, ActivityService activityService)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _activityService = activityService;
        }
        public async Task Consume(ConsumeContext<LatestImagesForActivitiesEvent> context)
        {
            var @event = context.Message;
            try
            {
                await _activityService.UpdateImagesOnActivities(@event.City, @event.ImagePackageByActivity);
                _logger.LogInformation("Successfully processed images for city: {city}", @event.City);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process images for city: {city}", @event.City);
                throw;  // Requeue the message in RabbitMQ
            }

        }
    }
}
