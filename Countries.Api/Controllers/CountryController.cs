

using Countries.Core.Features.Countries;
using Microsoft.AspNetCore.Mvc;
using CountryModel = Countries.Core.Features.Countries.Country;

namespace Country.Api.Controllers
{
    [ApiController]
    [Route("countries")]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        [HttpGet("getAll")]
        public async Task<ActionResult<List<CountryDto>>> GetAllCities()
        {
            var countryList = await _countryService.GetAll();
            if (countryList == null || !countryList.Any()) return NotFound($"No countries found .");
            return Ok(countryList);
        }


        [HttpPost("addCountries")]
        public async Task<ActionResult> AddCountries(List<CountryModel> countryList)
        {
            await _countryService.AddCountries(countryList);
            return Ok();
        }

    }
}

