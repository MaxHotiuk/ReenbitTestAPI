using ReenbitTest.Core.Entities;

namespace ReenbitTest.Core.Interfaces
{
    public interface IChatRepository
    {
        Task<IEnumerable<ChatRoom>> GetChatRoomsAsync(string userId);
        Task<ChatRoom> GetChatRoomByIdAsync(int chatRoomId);
        Task<ChatRoom> CreateChatRoomAsync(ChatRoom chatRoom);
        Task<IEnumerable<Message>> GetMessagesForChatRoomAsync(int chatRoomId, int page = 1, int pageSize = 20);
        Task<IEnumerable<(Message Message, bool IsRead)>> GetMessagesForChatRoomWithStatusAsync(int chatRoomId, string userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<(ChatRoom ChatRoom, int UnreadCount, string? LastMessage)>> GetLastMessagesWithUnreadCountForChatRoomsAsync(string userId);
        Task<bool> MarkAllAsReadByChatRoomIdAsync(int chatRoomId, string userId);
        Task<Message> AddMessageAsync(Message message);
        Task<bool> AddUserToChatRoomAsync(string userId, int chatRoomId);
        Task<bool> RemoveUserFromChatRoomAsync(string userId, int chatRoomId);
    }
}