
using Cities.Core.Features.Cities;
using Microsoft.AspNetCore.Mvc;

namespace TravelSwipe.Api.Controllers
{
    [ApiController]
    [Route("cities")]
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


        [HttpPost("addCities")]
        public async Task<ActionResult> AddCountries(List<City> cityList)
        {
            await _cityService.AddCities(cityList);
            return Ok(cityList);
        }

    }
}

