namespace ReenbitTest.Core.DTOs
{
    /// <summary>
    /// Data transfer object representing the response from authentication operations
    /// </summary>
    /// <remarks>
    /// This DTO is returned by authentication endpoints such as login and register.
    /// It contains authentication status, JWT token, user information, and any error messages.
    /// </remarks>
    public class AuthResponseDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether the authentication operation was successful
        /// </summary>
        /// <value>
        /// <c>true</c> if the authentication was successful; otherwise, <c>false</c>
        /// </value>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the JWT authentication token for API access
        /// </summary>
        /// <value>
        /// A string containing the JWT bearer token for authenticated requests
        /// </value>
        /// <remarks>
        /// The token should be included in the Authorization header for subsequent API requests.
        /// Format: "Bearer {token}"
        /// </remarks>
        public string Token { get; set; } = null!;

        /// <summary>
        /// Gets or sets the authenticated user's information
        /// </summary>
        /// <value>
        /// A <see cref="UserDto"/> containing the user's profile information
        /// </value>
        public UserDto User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the list of error messages if authentication failed
        /// </summary>
        /// <value>
        /// A list of error messages, or <c>null</c> if authentication was successful
        /// </value>
        public List<string>? Errors { get; set; }
    }
}