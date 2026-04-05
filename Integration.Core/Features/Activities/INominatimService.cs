using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Core.Features.Activities
{
    public interface INominatimService
    {
        public Task FetchLocation(CityGeoLocation location);

    }
}
