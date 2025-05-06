namespace ReenbitTest.Core.Entities
{
    /// <summary>
    /// Represents a message sent by a user in a chat room.
    /// Contains the message content, metadata, and sentiment analysis results.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the unique identifier for the message.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the content/text of the message.
        /// </summary>
        public string Content { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the date and time when the message was sent.
        /// </summary>
        public DateTime SentAt { get; set; }
        
        /// <summary>
        /// Gets or sets the unique identifier of the user who sent the message.
        /// Foreign key to the ApplicationUser entity.
        /// </summary>
        public string SenderId { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the user who sent this message.
        /// Navigation property to the associated ApplicationUser entity.
        /// </summary>
        public ApplicationUser Sender { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the unique identifier of the chat room where the message was sent.
        /// Foreign key to the ChatRoom entity. Nullable for system messages.
        /// </summary>
        public int? ChatRoomId { get; set; }
        
        /// <summary>
        /// Gets or sets the chat room where this message was sent.
        /// Navigation property to the associated ChatRoom entity.
        /// </summary>
        public ChatRoom ChatRoom { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the sentiment score of the message calculated by Azure Cognitive Services.
        /// Positive values indicate positive sentiment, negative values indicate negative sentiment.
        /// </summary>
        public string? SentimentScore { get; set; }
        
        /// <summary>
        /// Gets or sets the sentiment label of the message (e.g., "positive", "negative", "neutral").
        /// Derived from Azure Cognitive Services Text Analytics API.
        /// </summary>
        public string? SentimentLabel { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of read receipts for this message.
        /// Tracks which users have read this message and when.
        /// </summary>
        public ICollection<MessageRead> ReadBy { get; set; } = [];
    }
}