using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Contracts.Contracts;

namespace Cities.Core.Features.Cities
{
    public interface ICityMessageHandler
    {
        Task HandleCityDiscoveredAsync(CityDiscoveredEvent @event, CancellationToken cancellationToken = default);
    }
}
