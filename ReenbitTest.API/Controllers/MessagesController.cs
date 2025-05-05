using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReenbitTest.Core.DTOs;
using ReenbitTest.Core.Interfaces;
using System.Security.Claims;

namespace ReenbitTest.API.Controllers
{
    [ApiController]
    [Route("api/chatrooms/{chatRoomId}/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MessagesController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;

        public MessagesController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(
            int chatRoomId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(chatRoomId);

            if (chatRoom == null)
                return NotFound();

            if (!chatRoom.Users.Any(u => u.UserId == userId))
                return Forbid();

            var messages = await _chatRepository.GetMessagesForChatRoomAsync(chatRoomId, page, pageSize);

            var messageDtos = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SentAt = m.SentAt,
                SenderUserName = m.Sender.UserName!,
                SenderFullName = $"{m.Sender.FirstName} {m.Sender.LastName}",
                ChatRoomId = m.ChatRoomId!.Value,
                SentimentLabel = m.SentimentLabel
            });

            return Ok(messageDtos);
        }
    }
}