using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Activities.Core.Features.Cities
{
    public class City
    {
        public int Id { get; set; }
        public List<string>? AssociatedNames { get; set; }
        public List<string>? AssociatedSlugs { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? Country { get; set; } = string.Empty;

    }
}
