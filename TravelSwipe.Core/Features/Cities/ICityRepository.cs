using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Features.Countries;

namespace TravelSwipe.Core.Features.Cities
{
    public interface ICityRepository
    {
        Task<List<string>> GetCityNames();
        Task<List<City>> GetAll();
        Task<List<string>> FindCitiesNotRegisterd(List<String> cityList);
        Task AddBatch(List<City> cityList);

    }
}
