namespace ReenbitTest.Core.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new chat room
    /// </summary>
    /// <remarks>
    /// This DTO is used when creating a new chat room through the API,
    /// containing the room name and list of initial participants.
    /// </remarks>
    public class CreateChatRoomDto
    {
        /// <summary>
        /// Gets or sets the display name for the new chat room
        /// </summary>
        /// <value>
        /// A non-null string representing the chat room name
        /// </value>
        /// <remarks>
        /// The name should be descriptive and help users identify the purpose of the chat room
        /// </remarks>
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the list of user IDs to be added as initial members of the chat room
        /// </summary>
        /// <value>
        /// A list of string user identifiers, or null if no initial members are specified
        /// </value>
        /// <remarks>
        /// The request sender's user ID will be automatically added to this list if not already included.
        /// Each ID should correspond to an existing user in the system.
        /// </remarks>
        public List<string>? UserIds { get; set; }
    }
}