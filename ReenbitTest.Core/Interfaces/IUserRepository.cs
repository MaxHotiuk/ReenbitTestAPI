using ReenbitTest.Core.Entities;

namespace ReenbitTest.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetUsersAsync();
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetUserByUsernameAsync(string username);
        Task<bool> UpdateUserAsync(ApplicationUser user);
    }
}