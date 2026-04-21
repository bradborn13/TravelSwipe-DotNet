using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Shared.Models;

namespace TravelSwipe.Shared.Contracts
{
    public class FetchLatestImagesForLocation
    {
        public required string City { get; init; }
        public required List<string> Activities { get; init; }

    }
}
