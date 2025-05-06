using Microsoft.AspNetCore.Identity;
using ReenbitTest.Core.Entities;

namespace ReenbitTest.Infrastructure.Data
{
    /// <summary>
    /// Provides functionality to seed initial data into the application database.
    /// </summary>
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSeeder"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="userManager">The user manager for handling user operations.</param>
        /// <param name="roleManager">The role manager for handling role operations.</param>
        public DataSeeder(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Seeds the database with initial data if it's empty.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            if (!_context.Users.Any())
            {
                await SeedRoles();
                await SeedUsers();
                await SeedChatRooms();
                await SeedChatRoomUsers();
                await SeedMessages();
                await SeedMessageReads();
            }
        }

        /// <summary>
        /// Seeds initial roles into the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task SeedRoles()
        {
            var roles = new List<string> { "Admin", "User", "PremiumUser" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        /// <summary>
        /// Seeds initial users into the database with appropriate roles.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task SeedUsers()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow
                },
                new ApplicationUser
                {
                    UserName = "john.doe@example.com",
                    Email = "john.doe@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow
                },
                new ApplicationUser
                {
                    UserName = "jane.smith@example.com",
                    Email = "jane.smith@example.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow
                },
                new ApplicationUser
                {
                    UserName = "bob.johnson@example.com",
                    Email = "bob.johnson@example.com",
                    FirstName = "Bob",
                    LastName = "Johnson",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow
                }
            };

            foreach (var user in users)
            {
                var result = await _userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded)
                {
                    if (user.Email == "admin@example.com")
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                }
            }
        }

        /// <summary>
        /// Seeds initial chat rooms into the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task SeedChatRooms()
        {
            var chatRooms = new List<ChatRoom>
            {
                new ChatRoom
                {
                    Name = "General Chat",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new ChatRoom
                {
                    Name = "Tech Talk",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new ChatRoom
                {
                    Name = "Random",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new ChatRoom
                {
                    Name = "Private Support",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await _context.ChatRooms.AddRangeAsync(chatRooms);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds initial chat room user associations into the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task SeedChatRoomUsers()
        {
            var users = _context.Users.ToList();
            var chatRooms = _context.ChatRooms.ToList();

            var chatRoomUsers = new List<ChatRoomUser>();

            // All users in General Chat
            chatRoomUsers.AddRange(users.Select(user => new ChatRoomUser
            {
                UserId = user.Id,
                ChatRoomId = chatRooms[0].Id,
                JoinedAt = DateTime.UtcNow.AddDays(-9)
            }));

            // Tech Talk - admin and two users
            chatRoomUsers.AddRange(new[]
            {
                new ChatRoomUser
                {
                    UserId = users[0].Id,
                    ChatRoomId = chatRooms[1].Id,
                    JoinedAt = DateTime.UtcNow.AddDays(-4)
                },
                new ChatRoomUser
                {
                    UserId = users[1].Id,
                    ChatRoomId = chatRooms[1].Id,
                    JoinedAt = DateTime.UtcNow.AddDays(-4)
                },
                new ChatRoomUser
                {
                    UserId = users[2].Id,
                    ChatRoomId = chatRooms[1].Id,
                    JoinedAt = DateTime.UtcNow.AddDays(-3)
                }
            });

            // Random - all users
            chatRoomUsers.AddRange(users.Select(user => new ChatRoomUser
            {
                UserId = user.Id,
                ChatRoomId = chatRooms[2].Id,
                JoinedAt = DateTime.UtcNow.AddDays(-2)
            }));

            // Private Support - admin and one user
            chatRoomUsers.AddRange(new[]
            {
                new ChatRoomUser
                {
                    UserId = users[0].Id,
                    ChatRoomId = chatRooms[3].Id,
                    JoinedAt = DateTime.UtcNow.AddDays(-1)
                },
                new ChatRoomUser
                {
                    UserId = users[3].Id,
                    ChatRoomId = chatRooms[3].Id,
                    JoinedAt = DateTime.UtcNow.AddDays(-1)
                }
            });

            await _context.ChatRoomUsers.AddRangeAsync(chatRoomUsers);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds initial messages into the database for each chat room.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task SeedMessages()
        {
            var users = _context.Users.ToList();
            var chatRooms = _context.ChatRooms.ToList();

            var messages = new List<Message>
            {
                // General Chat messages
                new Message
                {
                    Content = "Welcome everyone to the General Chat!",
                    SentAt = DateTime.UtcNow.AddDays(-9),
                    SenderId = users[0].Id,
                    ChatRoomId = chatRooms[0].Id,
                    SentimentLabel = "neutral"
                },
                new Message
                {
                    Content = "Thanks for having me here!",
                    SentAt = DateTime.UtcNow.AddDays(-9),
                    SenderId = users[1].Id,
                    ChatRoomId = chatRooms[0].Id,
                    SentimentLabel = "positive"
                },
                new Message
                {
                    Content = "Looking forward to chatting with everyone!",
                    SentAt = DateTime.UtcNow.AddDays(-9),
                    SenderId = users[2].Id,
                    ChatRoomId = chatRooms[0].Id,
                    SentimentLabel = "positive"
                },

                // Tech Talk messages
                new Message
                {
                    Content = "Has anyone tried the new .NET 8 features?",
                    SentAt = DateTime.UtcNow.AddDays(-4),
                    SenderId = users[0].Id,
                    ChatRoomId = chatRooms[1].Id,
                    SentimentLabel = "neutral"
                },
                new Message
                {
                    Content = "Yes! The performance improvements are amazing.",
                    SentAt = DateTime.UtcNow.AddDays(-4),
                    SenderId = users[1].Id,
                    ChatRoomId = chatRooms[1].Id,
                    SentimentLabel = "positive"
                },
                new Message
                {
                    Content = "I'm still on .NET 6. Should I upgrade?",
                    SentAt = DateTime.UtcNow.AddDays(-3),
                    SenderId = users[2].Id,
                    ChatRoomId = chatRooms[1].Id,
                    SentimentLabel = "neutral"
                },

                // Random messages
                new Message
                {
                    Content = "What's everyone doing this weekend?",
                    SentAt = DateTime.UtcNow.AddDays(-2),
                    SenderId = users[3].Id,
                    ChatRoomId = chatRooms[2].Id,
                    SentimentLabel = "neutral"
                },
                new Message
                {
                    Content = "Probably just coding :)",
                    SentAt = DateTime.UtcNow.AddDays(-2),
                    SenderId = users[1].Id,
                    ChatRoomId = chatRooms[2].Id,
                    SentimentLabel = "positive"
                },

                // Private Support messages
                new Message
                {
                    Content = "I'm having issues with my account.",
                    SentAt = DateTime.UtcNow.AddDays(-1),
                    SenderId = users[3].Id,
                    ChatRoomId = chatRooms[3].Id,
                    SentimentLabel = "negative"
                },
                new Message
                {
                    Content = "I'll help you with that. What seems to be the problem?",
                    SentAt = DateTime.UtcNow.AddDays(-1),
                    SenderId = users[0].Id,
                    ChatRoomId = chatRooms[3].Id,
                    SentimentLabel = "positive"
                }
            };

            await _context.Messages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds message read receipts and updates the last seen information for chat room users.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task SeedMessageReads()
        {
            var messages = _context.Messages.ToList();
            var users = _context.Users.ToList();

            var messageReads = new List<MessageRead>();

            // Mark most messages as read by the sender
            foreach (var message in messages)
            {
                messageReads.Add(new MessageRead
                {
                    MessageId = message.Id,
                    UserId = message.SenderId,
                    ReadAt = message.SentAt.AddSeconds(1)
                });
            }

            // Mark some messages as read by other users in the chat
            // General Chat messages read by all
            var generalChatMessages = messages.Where(m => m.ChatRoomId == 1).ToList();
            foreach (var message in generalChatMessages)
            {
                foreach (var user in users.Where(u => u.Id != message.SenderId))
                {
                    messageReads.Add(new MessageRead
                    {
                        MessageId = message.Id,
                        UserId = user.Id,
                        ReadAt = message.SentAt.AddMinutes(new Random().Next(1, 60))
                    });
                }
            }

            // Tech Talk messages read by participants
            var techTalkMessages = messages.Where(m => m.ChatRoomId == 2).ToList();
            var techTalkUsers = users.Take(3).ToList(); // First 3 users are in Tech Talk
            foreach (var message in techTalkMessages)
            {
                foreach (var user in techTalkUsers.Where(u => u.Id != message.SenderId))
                {
                    messageReads.Add(new MessageRead
                    {
                        MessageId = message.Id,
                        UserId = user.Id,
                        ReadAt = message.SentAt.AddMinutes(new Random().Next(1, 60))
                    });
                }
            }

            await _context.MessageReads.AddRangeAsync(messageReads);
            await _context.SaveChangesAsync();

            // Update ChatRoomUser LastReadMessageId
            var chatRoomUsers = _context.ChatRoomUsers.ToList();
            foreach (var cru in chatRoomUsers)
            {
                var lastMessage = _context.Messages
                    .Where(m => m.ChatRoomId == cru.ChatRoomId)
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefault();

                if (lastMessage != null)
                {
                    cru.LastReadMessageId = lastMessage.Id;
                    cru.LastSeen = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}