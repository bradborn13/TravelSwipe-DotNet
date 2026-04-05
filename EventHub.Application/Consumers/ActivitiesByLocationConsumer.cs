using AutoMapper;
using EventHub.Application.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Models;

namespace EventHub.Application.Consumers
{
    public class ActivitiesByLocationConsumer : IConsumer<ActivitiesByLocationEvent>
    {
        private readonly ILogger<ActivitiesByLocationConsumer> _logger;
        private readonly IMapper _mapper;
        private readonly IHubContext<MessagingHub> _hubContext;

        public ActivitiesByLocationConsumer(
            ILogger<ActivitiesByLocationConsumer> logger,
            IMapper mapper,
            IHubContext<MessagingHub> hubContext)
        {
            _logger = logger;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<ActivitiesByLocationEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "In EventHub, Consuming ActivitiesByLocationEvent  for {DisplayName}, {EventCount} activiites found",
                @event.Location, @event.Activities.Count()
            );

            if (@event.Activities.Count() == 0)
            {
                _logger.LogInformation("ActivitiesByLocationEvent, in EventHub, found no Activities for location  {Location}. Skipping.", @event.Location);
                return;
            }

            //var activities = _mapper.Map<List<ActivityDto>>(@event.Activities);

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", @event.Activities);


            _logger.LogInformation("EventHub sent {ActivityAmount} activities for location {Location} to the client  ", @event.Activities.Count(), @event.Location);
        }
    }
}
