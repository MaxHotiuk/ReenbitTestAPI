namespace ReenbitTest.Core.DTOs
{
    /// <summary>
    /// Data transfer object representing a chat room with its properties and metadata
    /// </summary>
    /// <remarks>
    /// This DTO is used to transfer chat room information between client and server,
    /// including messages count, unread messages, and user information
    /// </remarks>
    public class ChatRoomDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the chat room
        /// </summary>
        /// <value>
        /// An integer representing the chat room's primary key
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of the chat room
        /// </summary>
        /// <value>
        /// A non-null string representing the chat room name
        /// </value>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the creation timestamp of the chat room
        /// </summary>
        /// <value>
        /// A DateTime representing when the chat room was created
        /// </value>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the collection of users participating in this chat room
        /// </summary>
        /// <value>
        /// A list of <see cref="UserDto"/> objects representing chat room members
        /// </value>
        public List<UserDto> Users { get; set; } = [];

        /// <summary>
        /// Gets or sets the total number of messages in this chat room
        /// </summary>
        /// <value>
        /// An integer representing the message count
        /// </value>
        public int MessageCount { get; set; }

        /// <summary>
        /// Gets or sets the number of unread messages for the current user
        /// </summary>
        /// <value>
        /// An integer representing the count of messages not yet read by the current user
        /// </value>
        public int UnreadCount { get; set; }

        /// <summary>
        /// Gets or sets the text content of the last message in the chat room
        /// </summary>
        /// <value>
        /// A string containing the message content, or null if no messages exist
        /// </value>
        public string? LastMessage { get; set; }
    }
}