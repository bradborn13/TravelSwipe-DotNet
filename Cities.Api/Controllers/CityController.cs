
using Cities.Core.Features.Cities;
using Microsoft.AspNetCore.Mvc;
using TravelSwipe.Core.Features.Countries;

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
            return Ok(cityList ?? new List<string>());
        }


        [HttpPost("addCities")]
        public async Task<ActionResult> AddCountries(List<City> cityList)
        {
            await _cityService.AddCities(cityList);
            return Ok(cityList);
        }

    }
}

