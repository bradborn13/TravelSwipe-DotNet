using AutoMapper;
using EventHub.Application.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Models;

namespace EventHub.Application.Consumer
{

    public class FoundActivityForLocationConsumer : IConsumer<FoundActivitiesForLocationEvent>
    {
        private readonly ILogger<FoundActivityForLocationConsumer> _logger;
        private readonly IMapper _mapper;
        private readonly IHubContext<MessagingHub> _hubContext;

        public FoundActivityForLocationConsumer(
            ILogger<FoundActivityForLocationConsumer> logger,
            IMapper mapper,
            IHubContext<MessagingHub> hubContext)
        {
            _logger = logger;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<FoundActivitiesForLocationEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "In EventHub, Consuming FoundActivitiesForLocationEvent  for {DisplayName}, {EventCount} activiites found",
                @event.Location, @event.Activities.Count()
            );

            if (@event.Activities.Count() == 0)
            {
                _logger.LogInformation("FoundActivitiesForLocationEvent, in EventHub, found no Activities for location  {Location}. Skipping.", @event.Location);
                return;
            }

            var activities = _mapper.Map<List<ActivityDto>>(@event.Activities);

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", @event.Activities);


            _logger.LogInformation("Evenmt hub sent {ActivityAmount} activities for location {Location} to the client  ", @event.Activities.Count(), @event.Location);
        }
    }
}
