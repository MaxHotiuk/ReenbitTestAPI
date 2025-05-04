using Microsoft.AspNetCore.Identity;

namespace ReenbitTest.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActive { get; set; }
        public ICollection<Message> Messages { get; set; } = [];
        public ICollection<ChatRoomUser> ChatRooms { get; set; } = [];
    }
}