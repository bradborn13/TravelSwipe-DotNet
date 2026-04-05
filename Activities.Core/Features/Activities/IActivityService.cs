
using TravelSwipe.Shared.Models;

namespace Activities.Core.Features.Activities
{
    public interface IActivityService
    {
        Task<List<ActivityDto>> GetActivitiesByCity(string city);
        //Task<List<ActivityDto>> ScrapePhotosForActivity(string city);
        //Task ScrapeCityAndCountryForActivity();

    }
}
