using ReenbitTest.Core.Entities;

namespace ReenbitTest.Core.Interfaces
{
    /// <summary>
    /// Defines data access operations for chat-related entities in the application.
    /// Handles chat rooms, messages, and user participation management.
    /// </summary>
    public interface IChatRepository
    {
        /// <summary>
        /// Retrieves all chat rooms that the specified user is a member of.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a collection of chat rooms.
        /// </returns>
        Task<IEnumerable<ChatRoom>> GetChatRoomsAsync(string userId);
        
        /// <summary>
        /// Retrieves a specific chat room by its ID.
        /// </summary>
        /// <param name="chatRoomId">The ID of the chat room to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the requested chat room.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown when the chat room with the specified ID is not found.</exception>
        Task<ChatRoom> GetChatRoomByIdAsync(int chatRoomId);
        
        /// <summary>
        /// Creates a new chat room.
        /// </summary>
        /// <param name="chatRoom">The chat room entity to create.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the created chat room with its assigned ID.
        /// </returns>
        Task<ChatRoom> CreateChatRoomAsync(ChatRoom chatRoom);
        
        /// <summary>
        /// Retrieves messages from a specific chat room with pagination support.
        /// </summary>
        /// <param name="chatRoomId">The ID of the chat room.</param>
        /// <param name="page">The page number (1-based). Default is 1.</param>
        /// <param name="pageSize">The number of messages per page. Default is 20.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a collection of messages for the specified chat room.
        /// </returns>
        Task<IEnumerable<Message>> GetMessagesForChatRoomAsync(int chatRoomId, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// Retrieves messages from a specific chat room with their read status for a particular user.
        /// </summary>
        /// <param name="chatRoomId">The ID of the chat room.</param>
        /// <param name="userId">The ID of the user whose read status is being checked.</param>
        /// <param name="page">The page number (1-based). Default is 1.</param>
        /// <param name="pageSize">The number of messages per page. Default is 20.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a collection of tuples with message and its read status for the specified user.
        /// </returns>
        Task<IEnumerable<(Message Message, bool IsRead)>> GetMessagesForChatRoomWithStatusAsync(int chatRoomId, string userId, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// Retrieves summary information for all chat rooms a user belongs to,
        /// including unread message counts and the last message in each room.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a collection of tuples with chat room, unread message count, and last message content.
        /// </returns>
        Task<IEnumerable<(ChatRoom ChatRoom, int UnreadCount, string? LastMessage)>> GetLastMessagesWithUnreadCountForChatRoomsAsync(string userId);
        
        /// <summary>
        /// Marks all unread messages in a chat room as read for a specific user.
        /// </summary>
        /// <param name="chatRoomId">The ID of the chat room.</param>
        /// <param name="userId">The ID of the user marking messages as read.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result is true if at least one message was marked as read; otherwise, false.
        /// </returns>
        Task<bool> MarkAllAsReadByChatRoomIdAsync(int chatRoomId, string userId);
        
        /// <summary>
        /// Adds a new message to a chat room.
        /// </summary>
        /// <param name="message">The message entity to add.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the added message with its assigned ID.
        /// </returns>
        Task<Message> AddMessageAsync(Message message);
        
        /// <summary>
        /// Adds a user to a chat room.
        /// </summary>
        /// <param name="userId">The ID of the user to add.</param>
        /// <param name="chatRoomId">The ID of the chat room.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result is true if the user was successfully added; otherwise, false.
        /// </returns>
        Task<bool> AddUserToChatRoomAsync(string userId, int chatRoomId);
        
        /// <summary>
        /// Removes a user from a chat room.
        /// </summary>
        /// <param name="userId">The ID of the user to remove.</param>
        /// <param name="chatRoomId">The ID of the chat room.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result is true if the user was successfully removed; otherwise, false.
        /// </returns>
        Task<bool> RemoveUserFromChatRoomAsync(string userId, int chatRoomId);
    }
}