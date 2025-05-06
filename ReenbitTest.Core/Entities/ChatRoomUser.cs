namespace ReenbitTest.Core.Entities
{
    /// <summary>
    /// Represents the many-to-many relationship between users and chat rooms.
    /// Tracks user membership in chat rooms and related metadata.
    /// </summary>
    public class ChatRoomUser
    {
        /// <summary>
        /// Gets or sets the unique identifier for the chat room user relationship.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user in this relationship.
        /// Foreign key to the ApplicationUser entity.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the user in this relationship.
        /// Navigation property to the associated ApplicationUser entity.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the unique identifier of the chat room in this relationship.
        /// Foreign key to the ChatRoom entity.
        /// </summary>
        public int ChatRoomId { get; set; }

        /// <summary>
        /// Gets or sets the chat room in this relationship.
        /// Navigation property to the associated ChatRoom entity.
        /// </summary>
        public ChatRoom ChatRoom { get; set; } = null!;

        /// <summary>
        /// Gets or sets the date and time when the user joined the chat room.
        /// </summary>
        public DateTime JoinedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the last message read by the user in this chat room.
        /// Used for tracking read status and unread message counts.
        /// </summary>
        public int? LastReadMessageId { get; set; }

        /// <summary>
        /// Gets or sets the last message read by the user in this chat room.
        /// Navigation property to the associated Message entity.
        /// </summary>
        public Message? LastReadMessage { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was last seen in the chat room.
        /// Used for tracking user activity within specific chat rooms.
        /// </summary>
        public DateTime? LastSeen { get; set; }
    }
}