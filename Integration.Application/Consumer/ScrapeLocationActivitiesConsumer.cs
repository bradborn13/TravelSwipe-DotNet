using Activities.Core.Features.Cities;
using Integration.Application.Integrations.Activities;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Shared.Contracts;


namespace Integration.Application.Consumer
{

    public class ScrapeLocationActivitiesConsumer : IConsumer<ScrapeLocationActivitiesEvent>
    {
        private readonly ILogger<ScrapeLocationActivitiesConsumer> _logger;
        private readonly FourSquareService _fsService;
        //private readonly IMapper _mapper;

        public ScrapeLocationActivitiesConsumer(
            ILogger<ScrapeLocationActivitiesConsumer> logger, FourSquareService fsService)
        {
            _logger = logger;
            _fsService = fsService;
        }

        public async Task Consume(ConsumeContext<ScrapeLocationActivitiesEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Consuming ScrapeLocationActivitiesEvent for {Location}",
                @event.Location
            );
            if (@event.Location != null)
            {
                await _fsService.SearchPlacesAsync(@event.Location);

            }
            else
            {
                _logger.LogInformation(
                "Aborted ScrapeLocationActivitiesEvent, location string is empty "
             );
            }



        }
    }
}
