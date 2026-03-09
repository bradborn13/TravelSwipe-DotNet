using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Features.Cities;

namespace TravelSwipe.Core.Features.Countries
{
    public interface ICountryRepository
    {
        Task<List<CountryDto>> GetAll();
        Task AddBatch(List<Country> countryList);
    }
}
