using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Core.Features.Cities
{
    public class City
    {
        public int Id { get; set; }
        public List<string>? Name { get; set; }
        public string NameClean { get; set; } = string.Empty;
        public string? Municipality { get; set; } = string.Empty;
        public List<string>? State { get; set; } = new();
        public string? Postcode { get; set; } = string.Empty;
        public string? Country { get; set; } = string.Empty;

    }
}
