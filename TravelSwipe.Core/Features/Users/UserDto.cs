using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Core.Features.Users
{
    public record UserDto(int Id,
        string FullName,
        string Email,
        DateTime CreatedAt,
        string FacebookId,
        string GoogleId,
        string Provider
        );
}

