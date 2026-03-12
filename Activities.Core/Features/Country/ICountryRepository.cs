using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Activities.Core.Features.Cities;

namespace TravelSwipe.Activities.Core.Features.Countries
{
    public interface ICountryRepository
    {
        Task AddBatch(List<Country> countryList);
    }
}
