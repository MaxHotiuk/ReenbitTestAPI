namespace ReenbitTest.Core.Entities
{
    public class ChatRoomUser
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public int ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
        public int? LastReadMessageId { get; set; }
        public Message? LastReadMessage { get; set; }
        public DateTime? LastSeen { get; set; }
    }
}