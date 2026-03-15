

using Cities.Core.Features.Countries;

namespace Cities.Core.Features.Cities
{
    public interface ICityRepository
    {
        Task<List<string>> GetCityNames();
        Task<List<City>> GetAll();
        Task<List<string>> FindCitiesNotRegisterd(List<String> cityList);
        Task AddBatch(List<City> cityList);
        Task<List<string>> CheckNotRegisteredByAssociatedNames(List<List<string>> countryList);


    }
}
