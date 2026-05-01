using Activities.Application.Hubs;
using Activities.Core.Features.Activities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TravelSwipe.Shared.Models;

namespace Activities.Api.Controllers
{
    [ApiController]
    [Route("activities")]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _activityService;
        private readonly IHubContext<MessagingHub> _hubContext;


        public ActivitiesController(IActivityService activityService, IHubContext<MessagingHub> hubContext)
        {
            _activityService = activityService;
            _hubContext = hubContext;
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<ActivityDto>>> GetActivities([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City parameter is required.");
            }
            var activities = await _activityService.GetActivitiesByCity(city);

            return Ok(activities);

        }
        [HttpPost("update/images")]
        public async Task<ActionResult<List<ActivityDto>>> UpdateImages([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City parameter is required.");
            }
            await _activityService.TriggerIntegrationImageUpdate(city);
            return Ok();
        }

    }
}
