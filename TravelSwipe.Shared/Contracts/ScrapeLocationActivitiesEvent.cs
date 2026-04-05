using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Shared.Contracts
{

    public record ScrapeLocationActivitiesEvent
    {
        public required string Location { get; init; }

    }
}
