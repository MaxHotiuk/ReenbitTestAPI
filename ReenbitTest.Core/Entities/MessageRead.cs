namespace ReenbitTest.Core.Entities
{
    public class MessageRead
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public Message Message { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public DateTime ReadAt { get; set; }
    }
}