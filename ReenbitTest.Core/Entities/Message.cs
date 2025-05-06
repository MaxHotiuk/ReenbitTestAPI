namespace ReenbitTest.Core.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public string SenderId { get; set; } = null!;
        public ApplicationUser Sender { get; set; } = null!;
        public int? ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;
        public string? SentimentScore { get; set; }
        public string? SentimentLabel { get; set; }
        public ICollection<MessageRead> ReadBy { get; set; } = [];
    }
}