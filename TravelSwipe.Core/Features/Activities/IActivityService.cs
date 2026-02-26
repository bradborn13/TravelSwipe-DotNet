using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Core.Features.Activities
{
    public interface IActivityService
    {
        Task<IEnumerable<ActivityDto>> GetActivitiesByCity(string city);
        Task<IEnumerable<ActivityDto>> ScrapePhotosForActivity(string city);

    }
}
