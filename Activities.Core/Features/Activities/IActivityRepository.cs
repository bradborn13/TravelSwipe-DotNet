using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Shared.Models;

namespace Activities.Core.Features.Activities
{
    public interface IActivityRepository
    {
        Task<List<Activity>> GetActivitiesByCity(string city);
        Task<List<Activity>> GetActivitiesWithoutImages(string city);
        Task<bool> AddImage(string activityName, string city, List<ImageURL> images);
        Task InsertActivityBatch(List<Activity> activities);
        Task<List<CityGeoLocation>> GetUniqueCityList();


    }
}

