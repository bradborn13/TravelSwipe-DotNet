using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cities.Core.Features.Cities
{
    public class City
    {
        public int Id { get; set; }
        public List<string>? AssociatedNames { get; set; }
        public List<string>? AssociatedSlugs { get; set; }
        public string? Municipality { get; set; } = string.Empty;
        public List<string>? State { get; set; } = new();
        public string? Postcode { get; set; } = string.Empty;
        public string? Country { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
