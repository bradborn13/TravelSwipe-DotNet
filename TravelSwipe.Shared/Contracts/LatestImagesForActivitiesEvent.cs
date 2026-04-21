using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Shared.Models;

namespace TravelSwipe.Shared.Contracts
{
    public class LatestImagesForActivitiesEvent
    {
        public required string City { get; init; }
        public required Dictionary<string, List<ImageURLMQ>> ImagePackageByActivity { get; init; }

    }
}
