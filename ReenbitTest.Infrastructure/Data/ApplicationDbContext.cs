using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReenbitTest.Core.Entities;

namespace ReenbitTest.Infrastructure.Data
{
    /// <summary>
    /// Database context for the application, providing access to all entity sets.
    /// Extends Identity's DbContext for user authentication and authorization.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the chat rooms in the database.
        /// </summary>
        public DbSet<ChatRoom> ChatRooms { get; set; }
        
        /// <summary>
        /// Gets or sets the messages in the database.
        /// </summary>
        public DbSet<Message> Messages { get; set; }
        
        /// <summary>
        /// Gets or sets the chat room user associations in the database.
        /// </summary>
        public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }
        
        /// <summary>
        /// Gets or sets the message read receipts in the database.
        /// </summary>
        public DbSet<MessageRead> MessageReads { get; set; }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types
        /// exposed in <see cref="DbSet{TEntity}"/> properties on the context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            
            // Configure Message -> Sender (User) relationship
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Message -> ChatRoom relationship
            modelBuilder.Entity<Message>()
                .HasOne(m => m.ChatRoom)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ChatRoomUser -> User relationship
            modelBuilder.Entity<ChatRoomUser>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.ChatRooms)
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ChatRoomUser -> ChatRoom relationship
            modelBuilder.Entity<ChatRoomUser>()
                .HasOne(cu => cu.ChatRoom)
                .WithMany(c => c.Users)
                .HasForeignKey(cu => cu.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure MessageRead -> Message relationship
            modelBuilder.Entity<MessageRead>()
                .HasOne(mr => mr.Message)
                .WithMany(m => m.ReadBy)
                .HasForeignKey(mr => mr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}