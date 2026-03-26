using Activities.Core.Features.Activities;
using AutoMapper;

using MassTransit;
using Microsoft.Extensions.Logging;
using TravelSwipe.Contracts.Contracts;

namespace Activities.Application.Consumer
{

    public class FoundActivityForLocationConsumer : IConsumer<FoundActivitiesForLocationEvent>
    {
        private readonly ILogger<FoundActivityForLocationConsumer> _logger;
        private readonly IActivityRepository _repo;
        private readonly IMapper _mapper;

        public FoundActivityForLocationConsumer(
            ILogger<FoundActivityForLocationConsumer> logger,
           IActivityRepository repo,
            IMapper mapper)
        {
            _logger = logger;
            _repo = repo;
            _mapper = mapper;
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
            await _repo.InsertActivityBatch(activities);

            _logger.LogInformation("FoundActivitiesForLocationEvent added {ActivityAmount} activities for location {Location}", @event.Activities.Count(), @event.Location);
        }
    }
}
