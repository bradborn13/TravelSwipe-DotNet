using Activities.Application.Services.Activities;
using Activities.Core.Features.Activities;
using TravelSwipe.Shared.Messaging;
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
    public class LatestImagesForActivitiesConsumer : IMessageHandler<LatestImagesForActivitiesEvent>
    {
        private readonly ILogger<LatestImagesForActivitiesConsumer> _logger;
        private readonly IActivityService _activityService;
        public LatestImagesForActivitiesConsumer(
       ILogger<LatestImagesForActivitiesConsumer> logger, IActivityService activityService)
        {
            _logger = logger;
            _activityService = activityService;
        }
        public async Task HandleAsync(LatestImagesForActivitiesEvent @event, CancellationToken cancellationToken)
        {
                        try
            {
                await _activityService.UpdateImagesOnActivities(@event.City, @event.ImagePackageByActivity);
                _logger.LogInformation("Successfully processed images for city: {city}", @event.City);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process images for city: {city}", @event.City);
                throw;  // Moves the message to activity-images-queue_error
            }

        }
    }
}
