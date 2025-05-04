namespace ReenbitTest.Core.DTOs
{
    public class CreateChatRoomDto
    {
        public string Name { get; set; } = null!;
        public List<string>? UserIds { get; set; }
    }
}