namespace ReenbitTest.Core.DTOs
{
    /// <summary>
    /// Data transfer object representing user information
    /// </summary>
    /// <remarks>
    /// This DTO is used for transferring user data across application layers
    /// without exposing sensitive information or implementation details.
    /// It contains essential user profile information that can be safely exposed to clients.
    /// </remarks>
    public class UserDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user
        /// </summary>
        /// <value>
        /// A non-null string representing the user's unique ID
        /// </value>
        /// <remarks>
        /// Maps to the Id property of ASP.NET Identity's IdentityUser class.
        /// Usually a GUID formatted as a string.
        /// </remarks>
        public string Id { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the username of the user
        /// </summary>
        /// <value>
        /// A non-null string representing the unique username
        /// </value>
        /// <remarks>
        /// Maps to the UserName property of ASP.NET Identity's IdentityUser class.
        /// Used for authentication and identification purposes.
        /// </remarks>
        public string UserName { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the email address of the user
        /// </summary>
        /// <value>
        /// A non-null string representing the user's email address
        /// </value>
        /// <remarks>
        /// Maps to the Email property of ASP.NET Identity's IdentityUser class.
        /// Used for communication and as an alternate login identifier.
        /// </remarks>
        public string Email { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the first name of the user
        /// </summary>
        /// <value>
        /// A non-null string representing the user's first name
        /// </value>
        /// <remarks>
        /// Used for personalization and display purposes in the application.
        /// </remarks>
        public string FirstName { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the last name of the user
        /// </summary>
        /// <value>
        /// A non-null string representing the user's last name
        /// </value>
        /// <remarks>
        /// Used for personalization and display purposes in the application.
        /// </remarks>
        public string LastName { get; set; } = null!;
        
        /// <summary>
        /// Gets the full name of the user
        /// </summary>
        /// <value>
        /// A string combining the user's first and last name
        /// </value>
        /// <remarks>
        /// Computed property that concatenates FirstName and LastName for display purposes.
        /// </remarks>
        public string FullName => $"{FirstName} {LastName}";
    }
}