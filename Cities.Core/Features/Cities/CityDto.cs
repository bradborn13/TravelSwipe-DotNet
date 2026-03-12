using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cities.Core.Features.Cities
{
    public record CityDto(int Id, string Name, string DisplayName, string Municipality, List<string> State, string Postcode, string Country, List<string> AssociatedSlugs, List<string> AssociatedNames);
}
