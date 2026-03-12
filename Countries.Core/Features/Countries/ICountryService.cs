
namespace Countries.Core.Features.Countries
{
    public interface ICountryService
    {
        Task<List<CountryDto>> GetAll();
        Task AddCountries(List<Country> cityList);


    }
}
