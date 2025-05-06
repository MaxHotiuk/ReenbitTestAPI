namespace ReenbitTest.Core.Entities
{
    /// <summary>
    /// Represents a chat room where users can exchange messages.
    /// </summary>
    public class ChatRoom
    {
        /// <summary>
        /// Gets or sets the unique identifier for the chat room.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the chat room.
        /// </summary>
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the date and time when the chat room was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of messages sent in this chat room.
        /// </summary>
        public ICollection<Message> Messages { get; set; } = [];
        
        /// <summary>
        /// Gets or sets the collection of users who are members of this chat room.
        /// Represents the many-to-many relationship between users and chat rooms.
        /// </summary>
        public ICollection<ChatRoomUser> Users { get; set; } = [];
    }
}