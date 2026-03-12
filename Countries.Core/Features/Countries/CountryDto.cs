using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Countries.Core.Features.Countries
{
    public record CountryDto(int Id, List<string> State, string DisplayName, string CountryCode);

}
