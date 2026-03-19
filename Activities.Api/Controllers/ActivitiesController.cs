using Microsoft.AspNetCore.Mvc;
using TravelSwipe.Activities.Core.Features.Activities;
using Activity = TravelSwipe.Activities.Core.Features.Activities.Activity;

namespace TravelSwipe.Api.Controllers
{
    [ApiController]
    [Route("activities")]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivitiesController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<ActivityDto>>> GetActivities([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City parameter is required.");
            }
            var activities = await _activityService.GetActivitiesByCity(city);
            return Ok(activities ?? new List<ActivityDto>());
        }
        [HttpPost("update/images")]
        public async Task<ActionResult<List<ActivityDto>>> UpdateImages([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City parameter is required.");
            }
            var activities = await _activityService.ScrapePhotosForActivity(city);
            return Ok(activities ?? new List<ActivityDto>());
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
