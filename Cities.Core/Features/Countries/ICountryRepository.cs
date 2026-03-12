using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cities.Core.Features.Countries
{
    public interface ICountryRepository
    {
        Task<List<Country>> GetAll();
        Task AddBatch(List<Country> countryList);
    }
}
