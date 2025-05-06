using Microsoft.EntityFrameworkCore;
using ReenbitTest.Core.Entities;
using ReenbitTest.Core.Interfaces;
using ReenbitTest.Infrastructure.Data;

namespace ReenbitTest.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of <see cref="IUserRepository"/> that provides data access operations
    /// for user-related entities using Entity Framework Core with Azure SQL Database.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="context">The database context used for data access.</param>
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId) ??
                throw new InvalidOperationException($"User with ID {userId} not found.");
        }

        /// <inheritdoc/>
        public async Task<ApplicationUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .SingleOrDefaultAsync(u => u.UserName == username) ??
                throw new InvalidOperationException($"User with username {username} not found.");
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}