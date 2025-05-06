namespace ReenbitTest.Core.DTOs
{
    /// <summary>
    /// Data transfer object for user registration operations
    /// </summary>
    /// <remarks>
    /// This DTO is used when registering new users through the authentication API.
    /// It contains all required information to create a new user account in the system
    /// using ASP.NET Identity for user management.
    /// </remarks>
    public class RegisterDto
    {
        /// <summary>
        /// Gets or sets the username for the new user account
        /// </summary>
        /// <value>
        /// A non-null string representing the unique username
        /// </value>
        /// <remarks>
        /// In ASP.NET Identity, this maps to the UserName property of IdentityUser.
        /// Must be unique across all users in the system.
        /// </remarks>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the email address for the new user account
        /// </summary>
        /// <value>
        /// A non-null string representing a valid email address
        /// </value>
        /// <remarks>
        /// Must be in valid email format and unique in the system.
        /// In ASP.NET Identity, this maps to the Email property of IdentityUser.
        /// Can be used for account confirmation and password recovery.
        /// </remarks>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Gets or sets the password for the new user account
        /// </summary>
        /// <value>
        /// A non-null string representing the user's password
        /// </value>
        /// <remarks>
        /// Password is subject to the complexity requirements configured in ASP.NET Identity.
        /// Typically requires a minimum length and a mix of character types.
        /// Will be hashed using ASP.NET Identity's password hasher before storage.
        /// Never returned in responses and should be transmitted over HTTPS only.
        /// </remarks>
        public string Password { get; set; } = null!;

        /// <summary>
        /// Gets or sets the first name of the user
        /// </summary>
        /// <value>
        /// A non-null string representing the user's first name
        /// </value>
        /// <remarks>
        /// Stored as a custom property in the ApplicationUser class that extends IdentityUser.
        /// Used for display purposes and personalization.
        /// </remarks>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the last name of the user
        /// </summary>
        /// <value>
        /// A non-null string representing the user's last name
        /// </value>
        /// <remarks>
        /// Stored as a custom property in the ApplicationUser class that extends IdentityUser.
        /// Used for display purposes and personalization.
        /// </remarks>
        public string LastName { get; set; } = null!;
    }
}