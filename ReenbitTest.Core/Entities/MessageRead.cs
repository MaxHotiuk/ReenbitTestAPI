namespace ReenbitTest.Core.Entities
{
    /// <summary>
    /// Represents a read receipt for a message.
    /// Tracks which users have read specific messages and when they were read.
    /// This entity supports the read status tracking functionality in the chat application.
    /// </summary>
    public class MessageRead
    {
        /// <summary>
        /// Gets or sets the unique identifier for the message read receipt.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the unique identifier of the message that was read.
        /// Foreign key to the Message entity.
        /// </summary>
        public int MessageId { get; set; }
        
        /// <summary>
        /// Gets or sets the message that was read.
        /// Navigation property to the associated Message entity.
        /// </summary>
        public Message Message { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the unique identifier of the user who read the message.
        /// Foreign key to the ApplicationUser entity.
        /// </summary>
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the user who read the message.
        /// Navigation property to the associated ApplicationUser entity.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the date and time when the message was read by the user.
        /// Used for analytics and chronological ordering of read receipts.
        /// </summary>
        public DateTime ReadAt { get; set; }
    }
}