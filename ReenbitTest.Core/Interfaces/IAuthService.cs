using ReenbitTest.Core.DTOs;
using ReenbitTest.Core.Entities;

namespace ReenbitTest.Core.Interfaces
{
    /// <summary>
    /// Defines authentication operations for the chat application.
    /// Handles user registration, login, and JWT token generation.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user in the application.
        /// </summary>
        /// <param name="registerDto">The DTO containing user registration information.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the authentication response with JWT token, user information, and expiration time.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown when registration fails due to validation issues or duplicate username/email.</exception>
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// </summary>
        /// <param name="loginDto">The DTO containing user login credentials.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the authentication response with JWT token, user information, and expiration time.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown when login fails due to invalid credentials.</exception>
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <returns>A string containing the JWT token.</returns>
        /// <remarks>
        /// The token contains claims for user ID, username, email, name, and roles.
        /// Token lifespan is typically set to 7 days but can be configured in the service implementation.
        /// </remarks>
        string GenerateJwtToken(ApplicationUser user);
    }
}