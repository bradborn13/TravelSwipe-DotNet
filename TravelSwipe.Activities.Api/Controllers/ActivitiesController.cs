using Microsoft.AspNetCore.Mvc;
using TravelSwipe.Activities.Core.Features.Activities;
using Activity = TravelSwipe.Activities.Core.Features.Activities.Activity;

namespace TravelSwipe.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivitiesController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<Activity>>> GetActivities([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City parameter is required.");
            }
            var activities = await _activityService.GetActivitiesByCity(city);
            if (activities == null || !activities.Any()) return NotFound($"No activities found in {city}.");
            return Ok(activities);
        }
        [HttpPost("update/images")]
        public async Task<ActionResult<IEnumerable<Activity>>> UpdateImages([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City parameter is required.");
            }
            var activities = await _activityService.ScrapePhotosForActivity(city);
            if (activities == null || !activities.Any()) return NotFound($"No activities found in {city}.");
            return Ok(activities);
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
