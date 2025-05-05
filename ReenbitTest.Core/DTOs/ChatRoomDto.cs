namespace ReenbitTest.Core.DTOs
{
    public class ChatRoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<UserDto> Users { get; set; } = [];
        public int MessageCount { get; set; }
    }
}