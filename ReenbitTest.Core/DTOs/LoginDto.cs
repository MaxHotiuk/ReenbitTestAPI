namespace ReenbitTest.Core.DTOs
{
    /// <summary>
    /// Data transfer object for user login operations
    /// </summary>
    /// <remarks>
    /// This DTO is used when authenticating users through the login endpoint.
    /// It contains the credentials required for authentication using ASP.NET Identity.
    /// </remarks>
    public class LoginDto
    {
        /// <summary>
        /// Gets or sets the email address of the user attempting to login
        /// </summary>
        /// <value>
        /// A non-null string representing the user's email address
        /// </value>
        /// <remarks>
        /// The email must be in a valid format and correspond to a registered user in the system.
        /// In ASP.NET Identity, this maps to the Email property of IdentityUser.
        /// </remarks>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Gets or sets the password for authentication
        /// </summary>
        /// <value>
        /// A non-null string representing the user's password
        /// </value>
        /// <remarks>
        /// The password is handled securely and never returned in responses.
        /// It is verified against the hashed version stored in ASP.NET Identity's user store.
        /// Password complexity requirements are enforced by ASP.NET Identity's password validator.
        /// </remarks>
        public string Password { get; set; } = null!;
    }
}