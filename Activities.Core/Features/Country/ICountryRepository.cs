using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Activities.Core.Features.Countries
{
    public interface ICountryRepository
    {
        Task AddBatch(List<Country> countryList);
    }
}
