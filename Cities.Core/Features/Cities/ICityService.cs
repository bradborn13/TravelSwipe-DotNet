using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cities.Core.Features.Cities
{
    public interface ICityService
    {
        Task<List<string>> GetAll();
        Task AddCities(List<City> cityList);
    }

}
