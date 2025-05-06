using Microsoft.AspNetCore.Identity;

namespace ReenbitTest.Core.Entities
{
    /// <summary>
    /// Represents a user in the chat application.
    /// Extends the ASP.NET Core Identity IdentityUser class with additional properties.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        public string FirstName { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        public string LastName { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the date and time when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the user was last active in the application.
        /// Used for tracking user online status and activity metrics.
        /// </summary>
        public DateTime LastActive { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of messages sent by this user.
        /// </summary>
        public ICollection<Message> Messages { get; set; } = [];
        
        /// <summary>
        /// Gets or sets the collection of chat rooms this user belongs to.
        /// Represents the many-to-many relationship between users and chat rooms.
        /// </summary>
        public ICollection<ChatRoomUser> ChatRooms { get; set; } = [];
    }
}