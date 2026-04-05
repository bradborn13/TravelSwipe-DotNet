using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Shared.Contracts;

namespace Cities.Core.Features.Cities
{
    public interface ICityMessageHandler
    {
        Task HandleCityRegisteredAsync(CityRegisteredEvent @event, CancellationToken cancellationToken = default);
    }
}
