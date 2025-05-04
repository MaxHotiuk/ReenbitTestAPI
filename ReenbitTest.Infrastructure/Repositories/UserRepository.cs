using Microsoft.EntityFrameworkCore;
using ReenbitTest.Core.Entities;
using ReenbitTest.Core.Interfaces;
using ReenbitTest.Infrastructure.Data;

namespace ReenbitTest.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId) ??
                throw new InvalidOperationException($"User with ID {userId} not found.");
        }

        public async Task<ApplicationUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .SingleOrDefaultAsync(u => u.UserName == username) ??
                throw new InvalidOperationException($"User with username {username} not found.");
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}