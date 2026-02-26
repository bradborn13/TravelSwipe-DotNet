using TravelSwipe.Core.Features.Users;

namespace TravelSwipe.Core.Core.Users
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
    }

}
