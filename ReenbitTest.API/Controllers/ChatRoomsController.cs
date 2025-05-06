using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReenbitTest.Core.DTOs;
using ReenbitTest.Core.Entities;
using ReenbitTest.Core.Interfaces;
using System.Security.Claims;

namespace ReenbitTest.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing chat rooms, including creation, retrieval, and user management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatRoomsController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRoomsController"/> class
        /// </summary>
        /// <param name="chatRepository">Repository for managing chat-related operations</param>
        /// <param name="userRepository">Repository for managing user-related operations</param>
        public ChatRoomsController(IChatRepository chatRepository, IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Gets all chat rooms for the authenticated user with last message and unread count information
        /// </summary>
        /// <returns>
        /// 200 OK with collection of chat room DTOs that the current user is part of
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatRoomDto>>> GetChatRooms()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var chatRooms = await _chatRepository.GetLastMessagesWithUnreadCountForChatRoomsAsync(userId);

            var chatRoomDtos = chatRooms.Select(c => new ChatRoomDto
            {
                Id = c.ChatRoom.Id,
                Name = c.ChatRoom.Name,
                CreatedAt = c.ChatRoom.CreatedAt,
                Users = c.ChatRoom.Users.Select(u => new UserDto
                {
                    Id = u.User.Id,
                    UserName = u.User.UserName!,
                    Email = u.User.Email!,
                    FirstName = u.User.FirstName,
                    LastName = u.User.LastName
                }).ToList(),
                MessageCount = c.ChatRoom.Messages?.Count() ?? 0,
                UnreadCount = c.UnreadCount,
                LastMessage = c.LastMessage
            });

            return Ok(chatRoomDtos);
        }

        /// <summary>
        /// Gets a specific chat room by its ID
        /// </summary>
        /// <param name="id">The ID of the chat room to retrieve</param>
        /// <returns>
        /// 200 OK with chat room details
        /// 403 Forbidden if user is not a member of the chat room
        /// 404 Not Found if chat room does not exist
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatRoomDto>> GetChatRoom(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(id);

            if (chatRoom == null)
                return NotFound();

            if (!chatRoom.Users.Any(u => u.UserId == userId))
                return Forbid();

            var chatRoomDto = new ChatRoomDto
            {
                Id = chatRoom.Id,
                Name = chatRoom.Name,
                CreatedAt = chatRoom.CreatedAt,
                Users = chatRoom.Users.Select(u => new UserDto
                {
                    Id = u.User.Id,
                    UserName = u.User.UserName!,
                    Email = u.User.Email!,
                    FirstName = u.User.FirstName,
                    LastName = u.User.LastName
                }).ToList(),
                MessageCount = chatRoom.Messages?.Count() ?? 0
            };

            return Ok(chatRoomDto);
        }

        /// <summary>
        /// Marks all messages in a chat room as read for the current user
        /// </summary>
        /// <param name="id">The ID of the chat room</param>
        /// <returns>
        /// 204 No Content if successful
        /// 400 Bad Request if operation fails
        /// 403 Forbidden if user is not a member of the chat room
        /// 404 Not Found if chat room does not exist
        /// </returns>
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAllAsRead(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(id);

            if (chatRoom == null)
                return NotFound();

            if (!chatRoom.Users.Any(u => u.UserId == userId))
                return Forbid();

            var result = await _chatRepository.MarkAllAsReadByChatRoomIdAsync(id, userId);
            if (!result)
                return BadRequest("Failed to mark messages as read");

            return NoContent();
        }

        /// <summary>
        /// Creates a new chat room
        /// </summary>
        /// <param name="createChatRoomDto">The chat room creation data</param>
        /// <returns>
        /// 201 Created with the newly created chat room details
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<ChatRoomDto>> CreateChatRoom(CreateChatRoomDto createChatRoomDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            
            // Make sure the creator is included in the list of users
            if (!createChatRoomDto.UserIds!.Contains(userId))
            {
                createChatRoomDto.UserIds.Add(userId);
            }

            var chatRoom = new ChatRoom
            {
                Name = createChatRoomDto.Name,
                CreatedAt = DateTime.UtcNow,
                Users = new List<ChatRoomUser>()
            };

            foreach (var id in createChatRoomDto.UserIds)
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user != null)
                {
                    chatRoom.Users.Add(new ChatRoomUser
                    {
                        UserId = id,
                        JoinedAt = DateTime.UtcNow
                    });
                }
            }

            var createdChatRoom = await _chatRepository.CreateChatRoomAsync(chatRoom);

            var chatRoomDto = new ChatRoomDto
            {
                Id = createdChatRoom.Id,
                Name = createdChatRoom.Name,
                CreatedAt = createdChatRoom.CreatedAt,
                Users = createdChatRoom.Users.Select(u => new UserDto
                {
                    Id = u.UserId,
                    UserName = u.User.UserName!,
                    Email = u.User.Email!,
                    FirstName = u.User.FirstName,
                    LastName = u.User.LastName
                }).ToList(),
                MessageCount = 0
            };

            return CreatedAtAction(nameof(GetChatRoom), new { id = chatRoom.Id }, chatRoomDto);
        }

        /// <summary>
        /// Adds a user to an existing chat room
        /// </summary>
        /// <param name="id">The ID of the chat room</param>
        /// <param name="userId">The ID of the user to add</param>
        /// <returns>
        /// 204 No Content if successful
        /// 400 Bad Request if operation fails
        /// 403 Forbidden if current user is not a member of the chat room
        /// 404 Not Found if chat room does not exist
        /// </returns>
        [HttpPost("{id}/users")]
        public async Task<IActionResult> AddUserToChatRoom(int id, [FromBody] string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(id);

            if (chatRoom == null)
                return NotFound();

            if (!chatRoom.Users.Any(u => u.UserId == currentUserId))
                return Forbid();

            var result = await _chatRepository.AddUserToChatRoomAsync(userId, id);
            if (!result)
                return BadRequest("Failed to add user to chat room");

            return NoContent();
        }

        /// <summary>
        /// Removes a user from a chat room
        /// </summary>
        /// <param name="id">The ID of the chat room</param>
        /// <param name="userId">The ID of the user to remove</param>
        /// <returns>
        /// 204 No Content if successful
        /// 400 Bad Request if operation fails
        /// 403 Forbidden if current user is not authorized to remove the specified user
        /// 404 Not Found if chat room does not exist
        /// </returns>
        [HttpDelete("{id}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromChatRoom(int id, string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(id);

            if (chatRoom == null)
                return NotFound();

            // Only allow removing oneself or if current user is the creator
            if (userId != currentUserId && !chatRoom.Users.Any(u => u.UserId == currentUserId))
                return Forbid();

            var result = await _chatRepository.RemoveUserFromChatRoomAsync(userId, id);
            if (!result)
                return BadRequest("Failed to remove user from chat room");

            return NoContent();
        }
    }
}