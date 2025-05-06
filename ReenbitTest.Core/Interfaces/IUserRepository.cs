using ReenbitTest.Core.Entities;

namespace ReenbitTest.Core.Interfaces
{
    /// <summary>
    /// Defines data access operations for user-related entities in the application.
    /// Provides functionality to retrieve and update user information.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves all users in the application.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a collection of all users.
        /// </returns>
        Task<IEnumerable<ApplicationUser>> GetUsersAsync();

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the requested user.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown when a user with the specified ID is not found.</exception>
        Task<ApplicationUser> GetUserByIdAsync(string userId);

        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the requested user.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown when a user with the specified username is not found.</exception>
        Task<ApplicationUser> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Updates an existing user's information.
        /// </summary>
        /// <param name="user">The user entity with updated information.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result is true if the user was successfully updated; otherwise, false.
        /// </returns>
        Task<bool> UpdateUserAsync(ApplicationUser user);
    }
}