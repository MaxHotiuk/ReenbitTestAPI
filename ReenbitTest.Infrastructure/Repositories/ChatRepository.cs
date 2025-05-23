using Microsoft.EntityFrameworkCore;
using ReenbitTest.Core.Entities;
using ReenbitTest.Core.Interfaces;
using ReenbitTest.Infrastructure.Data;

namespace ReenbitTest.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of <see cref="IChatRepository"/> that provides data access operations
    /// for chat-related entities using Entity Framework Core with Azure SQL Database.
    /// </summary>
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRepository"/> class.
        /// </summary>
        /// <param name="context">The database context used for data access.</param>
        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatRoom>> GetChatRoomsAsync(string userId)
        {
            return await _context.ChatRooms
                .Include(c => c.Users)
                    .ThenInclude(u => u.User)
                .Where(c => c.Users.Any(u => u.UserId == userId))
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<ChatRoom> GetChatRoomByIdAsync(int chatRoomId)
        {
            var chatRoom = await _context.ChatRooms
                .Include(c => c.Users)
                    .ThenInclude(u => u.User)
                .SingleOrDefaultAsync(c => c.Id == chatRoomId) ?? 
                    throw new InvalidOperationException($"ChatRoom with ID {chatRoomId} not found.");
            return chatRoom;
        }

        /// <inheritdoc/>
        public async Task<ChatRoom> CreateChatRoomAsync(ChatRoom chatRoom)
        {
            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();
            return chatRoom;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Message>> GetMessagesForChatRoomAsync(int chatRoomId, int page = 1, int pageSize = 20)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Message> AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        /// <inheritdoc/>
        public async Task<bool> AddUserToChatRoomAsync(string userId, int chatRoomId)
        {
            var chatRoomUser = new ChatRoomUser
            {
                UserId = userId,
                ChatRoomId = chatRoomId,
                JoinedAt = DateTime.UtcNow
            };

            _context.ChatRoomUsers.Add(chatRoomUser);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveUserFromChatRoomAsync(string userId, int chatRoomId)
        {
            var chatRoomUser = await _context.ChatRoomUsers
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ChatRoomId == chatRoomId);

            if (chatRoomUser == null)
                return false;

            _context.ChatRoomUsers.Remove(chatRoomUser);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<(Message Message, bool IsRead)>> GetMessagesForChatRoomWithStatusAsync(int chatRoomId, string userId, int page = 1, int pageSize = 20)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new 
                { 
                    Message = m, 
                    IsRead = _context.MessageReads.Any(r => r.MessageId == m.Id && r.UserId == userId) 
                })
                .OrderBy(m => m.Message.SentAt)
                .ToListAsync()
                .ContinueWith(task => task.Result.Select(m => (m.Message, m.IsRead)));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<(ChatRoom ChatRoom, int UnreadCount, string? LastMessage)>> GetLastMessagesWithUnreadCountForChatRoomsAsync(string userId)
        {
            return await _context.ChatRooms
                .Include(c => c.Users)
                    .ThenInclude(u => u.User)
                .Where(c => c.Users.Any(u => u.UserId == userId))
                .Select(c => new
                {
                    ChatRoom = c,
                    UnreadCount = _context.Messages
                        .Where(m => m.ChatRoomId == c.Id && !_context.MessageReads.Any(r => r.MessageId == m.Id && r.UserId == userId))
                        .Count(),
                    LastMessage = _context.Messages
                        .Where(m => m.ChatRoomId == c.Id)
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => m.Content)
                        .FirstOrDefault()
                })
                .ToListAsync()
                .ContinueWith(task => task.Result.Select(r => (r.ChatRoom, r.UnreadCount, r.LastMessage)));
        }

        /// <inheritdoc/>
        public async Task<bool> MarkAllAsReadByChatRoomIdAsync(int chatRoomId, string userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ChatRoomId == chatRoomId && !_context.MessageReads.Any(r => r.MessageId == m.Id && r.UserId == userId))
                .ToListAsync();

            foreach (var message in messages)
            {
                _context.MessageReads.Add(new MessageRead
                {
                    MessageId = message.Id,
                    UserId = userId,
                    ReadAt = DateTime.UtcNow
                });
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}