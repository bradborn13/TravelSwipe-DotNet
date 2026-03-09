using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Core.Features.Cities
{
    public interface ICityService
    {
        Task<List<string>> GetAll();
    }

}
