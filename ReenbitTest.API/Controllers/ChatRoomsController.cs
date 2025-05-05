using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReenbitTest.Core.DTOs;
using ReenbitTest.Core.Entities;
using ReenbitTest.Core.Interfaces;
using System.Security.Claims;

namespace ReenbitTest.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatRoomsController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;

        public ChatRoomsController(IChatRepository chatRepository, IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatRoomDto>>> GetChatRooms()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var chatRooms = await _chatRepository.GetChatRoomsAsync(userId);

            var chatRoomDtos = chatRooms.Select(c => new ChatRoomDto
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt,
                Users = c.Users.Select(u => new UserDto
                {
                    Id = u.User.Id,
                    UserName = u.User.UserName!,
                    Email = u.User.Email!,
                    FirstName = u.User.FirstName,
                    LastName = u.User.LastName
                }).ToList(),
                MessageCount = c.Messages?.Count() ?? 0
            });

            return Ok(chatRoomDtos);
        }

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