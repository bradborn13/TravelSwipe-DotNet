using AutoMapper;
using EventHub.Application.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Models;

namespace EventHub.Application.Consumer
{

    public class LatestImagesForActivitiesConsumer : IConsumer<LatestImagesForActivitiesEvent>
    {
        private readonly ILogger<LatestImagesForActivitiesConsumer> _logger;
        private readonly IMapper _mapper;
        private readonly IHubContext<MessagingHub> _hubContext;

        public LatestImagesForActivitiesConsumer(
            ILogger<LatestImagesForActivitiesConsumer> logger,
            IMapper mapper,
            IHubContext<MessagingHub> hubContext)
        {
            _logger = logger;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<LatestImagesForActivitiesEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "In EventHub, Consuming LatestImagesForActivitiesEvent  for {DisplayName}, {EventCount} activiites found",
                @event.City, @event.ImagePackageByActivity.Count()
            );

            if (@event.ImagePackageByActivity.Count() == 0)
            {
                _logger.LogInformation("LatestImagesForActivitiesEvent, in EventHub, found no Activities for location  {Location}. Skipping.", @event.City);
                return;
            }
            //var activities = _mapper.Map<List<ActivityDto>>(@event.ImagePackageByActivity);

            await _hubContext.Clients.All.SendAsync("UpdatedImagesForActivities", @event);


            _logger.LogInformation("Evenmt hub sent {ActivityAmount} activities for location {Location} to the client  ", @event.ImagePackageByActivity.Count(), @event.City);
        }
    }
}
