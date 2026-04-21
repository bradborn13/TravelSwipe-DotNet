

using Microsoft.Extensions.Logging;
using TravelSwipe.Shared.Models;

namespace Activities.Core.Features.Activities
{
    public interface IActivityService
    {
        Task<List<ActivityDto>> GetActivitiesByCity(string city);
        Task UpdateImagesOnActivities(string city, Dictionary<string, List<ImageURLMQ>> imagePackageByActivities);
        //Task<List<ActivityDto>> ScrapePhotosForActivity(string city);
        //Task ScrapeCityAndCountryForActivity();

    }
}
