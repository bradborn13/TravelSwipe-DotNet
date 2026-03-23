using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Activities.Core.Features.Activities
{
    public interface IActivityService
    {
        Task<List<ActivityDto>> GetActivitiesByCity(string city);
        Task<List<ActivityDto>> ScrapePhotosForActivity(string city);
        Task ScrapeCityAndCountryForActivity();

    }
}
