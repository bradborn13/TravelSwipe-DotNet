using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Core.Users;
using TravelSwipe.Core.Features.Users;

namespace TravelSwipe.Application.Services
{

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users.Select(u => new UserDto(u.Id, u.Fullname, u.Email, u.CreatedAt, u.FacebookId, u.GoogleId, u.Provider));
        }

    }
}
