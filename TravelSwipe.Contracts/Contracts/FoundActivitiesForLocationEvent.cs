using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Contracts.Models;

namespace TravelSwipe.Contracts.Contracts
{
    public class FoundActivitiesForLocationEvent
    {
        public required string Location { get; init; }
        public required List<ActivityMQ> Activities { get; init; }

    }
}
