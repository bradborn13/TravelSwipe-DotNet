using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Features.Users;

namespace TravelSwipe.Core.Features.Activities
{
    public interface IActivityRepository
    {
        Task<IEnumerable<Activity>> GetActivitiesByCity(string city);
        Task<List<Activity>> GetActivitiesWithoutImages(string city);
        Task<bool> AddImage(string activityName, string city, List<ImageURL> images);
        Task InsertActivityBatch(List<Activity> activities);
        Task<List<CityGeoLocation>> GetUniqueCountryList();


    }
}

