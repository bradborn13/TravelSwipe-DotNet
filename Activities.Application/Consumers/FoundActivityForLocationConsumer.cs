using Activities.Application.Hubs;
using Activities.Core.Features.Activities;
using AutoMapper;

using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TravelSwipe.Shared.Contracts;

namespace Activities.Application.Consumer
{

    public class FoundActivityForLocationConsumer : IConsumer<FoundActivitiesForLocationEvent>
    {
        private readonly ILogger<FoundActivityForLocationConsumer> _logger;
        private readonly IActivityRepository _repo;
        private readonly IMapper _mapper;
        private readonly IHubContext<MessagingHub> _hubContext;

        public FoundActivityForLocationConsumer(
            ILogger<FoundActivityForLocationConsumer> logger,
           IActivityRepository repo,
            IMapper mapper,
IHubContext<MessagingHub> hubContext)
        {
            _logger = logger;
            _repo = repo;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<FoundActivitiesForLocationEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Consuming FoundActivitiesForLocationEvent for {DisplayName}, {EventCount} activiites found",
                @event.Location, @event.Activities.Count()
            );

            if (@event.Activities.Count() == 0)
            {
                _logger.LogInformation("FoundActivitiesForLocationEvent found no Activities for location  {Location}. Skipping.", @event.Location);
                return;
            }

            var activities = _mapper.Map<List<Activity>>(@event.Activities);
            //experiment- signalR
            //foreach (var activity in activities)
            //{
            //    await _hubContext.Clients.All.SendAsync("ReceiveMessage", activity.Name);

            //}
            await _repo.InsertActivityBatch(activities);

            _logger.LogInformation("FoundActivitiesForLocationEvent added {ActivityAmount} activities for location {Location}", @event.Activities.Count(), @event.Location);
        }
    }
}
