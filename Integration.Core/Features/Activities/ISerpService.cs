using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Shared.Models;

namespace Integration.Core.Features.Activities
{
    public interface ISerpService
    {
        public Task<List<ImageURL>> GetImages(string activityName, string city);

    }
}
