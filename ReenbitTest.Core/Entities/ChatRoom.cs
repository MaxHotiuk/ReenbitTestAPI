namespace ReenbitTest.Core.Entities
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public ICollection<Message> Messages { get; set; } = [];
        public ICollection<ChatRoomUser> Users { get; set; } = [];
    }
}