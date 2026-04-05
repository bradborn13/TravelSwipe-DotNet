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
            foreach (var activity in activities)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", new { Sender = "Coder", Content = activity.Name, SentTime = DateTime.Now });

            }
            return Ok(new List<ActivityDto>());

            //return Ok(activities ?? new List<ActivityDto>());
        }
        [HttpPost("update/images")]
        public async Task<ActionResult<List<ActivityDto>>> UpdateImages([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City parameter is required.");
            }
            //var activities = await _activityService.ScrapePhotosForActivity(city);
            //return Ok(activities ?? new List<ActivityDto>());
            return BadRequest();
        }
        //[HttpGet("find/country")]
        //public async Task<IActionResult> FetchLocationDetails()
        //{
        //    //var result = _activityService.
        //    //return BadRequest("City parameter is required.");

        //    //return NoContent();
        //}
    }
}
