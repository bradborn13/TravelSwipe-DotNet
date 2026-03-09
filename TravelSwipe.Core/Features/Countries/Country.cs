using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Core.Features.Countries
{
    public class Country
    {
        public int Id { get; set; }
        public List<string>? Name { get; set; }
        public string NameClean { get; set; } = string.Empty;
        public string? CountryCode { get; set; } = string.Empty;

    }
}
