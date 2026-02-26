using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Core.Features.Cities
{
    public record CityDto(int Id, string Name, string NameClean, string Municipality, List<string> State, string Postcode, string Country);
}
