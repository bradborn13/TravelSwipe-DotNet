
using Microsoft.AspNetCore.Mvc;
using TravelSwipe.Core.Features.Activities;
using TravelSwipe.Core.Features.Cities;

namespace TravelSwipe.Api.Controllers
{
    [ApiController]
    [Route("city")]
    public class CityController : ControllerBase
    {


        private readonly ICityService _cityService;

        public CityController(ICityService cityService)
        {
            _cityService = cityService;
        }

        [HttpGet("getAll")]
        public async Task<ActionResult<List<string>>> GetAllCities()
        {
            var cityList = await _cityService.GetAll();
            if (cityList == null || !cityList.Any()) return NotFound($"No cities found .");
            return Ok(cityList);
        }

    }
}

