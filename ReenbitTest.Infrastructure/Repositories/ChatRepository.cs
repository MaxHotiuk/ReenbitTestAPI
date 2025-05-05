using Microsoft.EntityFrameworkCore;
using ReenbitTest.Core.Entities;
using ReenbitTest.Core.Interfaces;
using ReenbitTest.Infrastructure.Data;

namespace ReenbitTest.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChatRoom>> GetChatRoomsAsync(string userId)
        {
            return await _context.ChatRooms
                .Include(c => c.Users)
                    .ThenInclude(u => u.User)
                .Where(c => c.Users.Any(u => u.UserId == userId))
                .ToListAsync();
        }

        public async Task<ChatRoom> GetChatRoomByIdAsync(int chatRoomId)
        {
            var chatRoom = await _context.ChatRooms
                .Include(c => c.Users)
                    .ThenInclude(u => u.User)
                .SingleOrDefaultAsync(c => c.Id == chatRoomId) ?? 
                    throw new InvalidOperationException($"ChatRoom with ID {chatRoomId} not found.");
            return chatRoom;
        }

        public async Task<ChatRoom> CreateChatRoomAsync(ChatRoom chatRoom)
        {
            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();
            return chatRoom;
        }

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

        public async Task<Message> AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

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

        public async Task<bool> RemoveUserFromChatRoomAsync(string userId, int chatRoomId)
        {
            var chatRoomUser = await _context.ChatRoomUsers
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ChatRoomId == chatRoomId);

            if (chatRoomUser == null)
                return false;

            _context.ChatRoomUsers.Remove(chatRoomUser);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}