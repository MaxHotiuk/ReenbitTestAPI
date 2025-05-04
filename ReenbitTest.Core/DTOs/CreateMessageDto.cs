namespace ReenbitTest.Core.DTOs
{
    public class CreateMessageDto
    {
        public string Content { get; set; } = null!;
        public int ChatRoomId { get; set; }
    }
}