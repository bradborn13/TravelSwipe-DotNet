using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cities.Core.Features.Countries
{
    public class Country
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? CountryCode { get; set; } = string.Empty;

    }
}
