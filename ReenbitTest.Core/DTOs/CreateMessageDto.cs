namespace ReenbitTest.Core.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new message in a chat room
    /// </summary>
    /// <remarks>
    /// This DTO is used when sending a new message to a specific chat room through the API.
    /// It contains the message content and the ID of the target chat room.
    /// </remarks>
    public class CreateMessageDto
    {
        /// <summary>
        /// Gets or sets the content of the message
        /// </summary>
        /// <value>
        /// A non-null string representing the text content of the message
        /// </value>
        /// <remarks>
        /// The content should not exceed the maximum allowed length for messages
        /// and should comply with any content moderation policies.
        /// </remarks>
        public string Content { get; set; } = null!;

        /// <summary>
        /// Gets or sets the ID of the chat room where the message will be sent
        /// </summary>
        /// <value>
        /// An integer representing the unique identifier of the target chat room
        /// </value>
        /// <remarks>
        /// The chat room ID must correspond to an existing chat room in the system.
        /// The sender must be a member of this chat room to successfully send messages.
        /// </remarks>
        public int ChatRoomId { get; set; }
    }
}