# ReenbitTest Chat Application - Developer Documentation

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture](#2-architecture)
   - [Solution Structure](#21-solution-structure)
   - [Design Patterns](#22-design-patterns)
3. [Core Domain](#3-core-domain)
   - [Entities](#31-entities)
   - [DTOs](#32-dtos)
   - [Interfaces](#33-interfaces)
4. [Infrastructure Layer](#4-infrastructure-layer)
   - [Data Context](#41-data-context)
   - [Repository Implementations](#42-repository-implementations)
   - [Services](#43-services)
   - [Data Seeding](#44-data-seeding)
5. [API Layer](#5-api-layer)
   - [Controllers](#51-controllers)
   - [SignalR Hubs](#52-signalr-hubs)
   - [Authentication & Authorization](#53-authentication--authorization)
6. [Azure Integration](#6-azure-integration)
   - [Azure SQL Database](#61-azure-sql-database)
   - [Azure SignalR Service](#62-azure-signalr-service)
   - [Azure Cognitive Services](#63-azure-cognitive-services)
   - [Connection String Management](#64-connection-string-management)
7. [Real-time Communication](#7-real-time-communication)
   - [SignalR Implementation](#71-signalr-implementation)
   - [Connection Management](#72-connection-management)
   - [Message Broadcasting](#73-message-broadcasting)
8. [Sentiment Analysis](#8-sentiment-analysis)
   - [Analysis Process](#81-analysis-process)
   - [Score Calculation](#82-score-calculation)
9. [Database Schema](#9-database-schema)
   - [Entity Relationships](#91-entity-relationships)
   - [Indexes](#92-indexes)
   - [Migration Strategy](#93-migration-strategy)
10. [Project Setup](#10-project-setup)
    - [Prerequisites](#101-prerequisites)
    - [Environment Configuration](#102-environment-configuration)
    - [Database Setup](#103-database-setup)
    - [Running the Application](#104-running-the-application)
11. [API Documentation](#11-api-documentation)
    - [Authentication Endpoints](#111-authentication-endpoints)
    - [User Endpoints](#112-user-endpoints)
    - [Chat Room Endpoints](#113-chat-room-endpoints)
    - [Message Endpoints](#114-message-endpoints)
12. [Deployment](#12-deployment)
    - [Azure Deployment](#121-azure-deployment)
    - [CI/CD Pipeline](#122-cicd-pipeline)
13. [Common Development Tasks](#13-common-development-tasks)
    - [Adding a New Entity](#131-adding-a-new-entity)
    - [Creating a Migration](#132-creating-a-migration)
    - [Adding a New API Endpoint](#133-adding-a-new-api-endpoint)

## 1. Project Overview

ReenbitTest is a real-time chat application built with ASP.NET Core that enables users to communicate through chat rooms. The application is designed with a focus on real-time messaging, user management, and sentiment analysis of messages.

### Key Features

- User authentication and authorization using JWT tokens
- Chat room management (creation, joining, leaving)
- Real-time messaging using SignalR
- Message sentiment analysis using Azure Cognitive Services
- Read receipts for messages
- Message history with pagination
- Unread message tracking

## 2. Architecture

### 2.1 Solution Structure

The solution follows a clean architecture pattern separated into three main projects:

- **ReenbitTest.Core**: Contains domain entities, interfaces, and DTOs
- **ReenbitTest.Infrastructure**: Contains implementations of repositories, services, and data access
- **ReenbitTest.API**: Contains controllers, SignalR hubs, and application configuration

### 2.2 Design Patterns

- **Repository Pattern**: Abstracts data access layer, allowing easy switching between data sources
- **Dependency Injection**: Used throughout the solution for loose coupling
- **DTO Pattern**: Data Transfer Objects used to encapsulate data passed between layers
- **Unit of Work**: Implied through the use of Entity Framework's DbContext

## 3. Core Domain

### 3.1 Entities

#### ApplicationUser

```csharp
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActive { get; set; }
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<ChatRoomUser> ChatRooms { get; set; } = [];
}
```

Extends ASP.NET Identity's IdentityUser with additional user profile information.

#### ChatRoom

```csharp
public class ChatRoom
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<ChatRoomUser> Users { get; set; } = [];
}
```

Represents a chat room where users can send messages.

#### Message

```csharp
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
```

Represents a message sent by a user in a chat room, including sentiment analysis information.

#### ChatRoomUser

```csharp
public class ChatRoomUser
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public int ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
    public DateTime? LastSeen { get; set; }
    public int? LastReadMessageId { get; set; }
    public Message? LastReadMessage { get; set; }
}
```

Many-to-many relationship between users and chat rooms, tracking user participation.

#### MessageRead

```csharp
public class MessageRead
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public Message Message { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public DateTime ReadAt { get; set; }
}
```

Tracks which messages have been read by which users and when.

### 3.2 DTOs

#### UserDto

```csharp
public class UserDto
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
}
```

Used to transfer user data between layers without exposing sensitive information.

#### ChatRoomDto

```csharp
public class ChatRoomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<UserDto> Users { get; set; } = [];
    public int MessageCount { get; set; }
    public int UnreadCount { get; set; }
    public string? LastMessage { get; set; }
}
```

Represents a chat room with its users and message statistics.

#### MessageDto

```csharp
public class MessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public string SenderUserName { get; set; } = null!;
    public string SenderFullName { get; set; } = null!;
    public int ChatRoomId { get; set; }
    public string? SentimentLabel { get; set; }
    public bool IsRead { get; set; }
}
```

Used to transfer message data to the client, including sender information and sentiment analysis.

#### CreateChatRoomDto

```csharp
public class CreateChatRoomDto
{
    public string Name { get; set; } = null!;
    public List<string>? UserIds { get; set; }
}
```

Used when creating a new chat room through the API.

#### CreateMessageDto

```csharp
public class CreateMessageDto
{
    public string Content { get; set; } = null!;
    public int ChatRoomId { get; set; }
}
```

Used when sending a new message through the API.

#### RegisterDto

```csharp
public class RegisterDto
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}
```

Used for user registration.

#### LoginDto

```csharp
public class LoginDto
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}
```

Used for user login.

#### AuthResultDto

```csharp
public class AuthResultDto
{
    public string Token { get; set; } = null!;
    public UserDto User { get; set; } = null!;
    public DateTime Expiration { get; set; }
}
```

Contains authentication result information, including JWT token.

### 3.3 Interfaces

#### IChatRepository

```csharp
public interface IChatRepository
{
    Task<IEnumerable<ChatRoom>> GetChatRoomsAsync(string userId);
    Task<ChatRoom> GetChatRoomByIdAsync(int chatRoomId);
    Task<ChatRoom> CreateChatRoomAsync(ChatRoom chatRoom);
    Task<IEnumerable<Message>> GetMessagesForChatRoomAsync(int chatRoomId, int page = 1, int pageSize = 20);
    Task<IEnumerable<(Message Message, bool IsRead)>> GetMessagesForChatRoomWithStatusAsync(int chatRoomId, string userId, int page = 1, int pageSize = 20);
    Task<IEnumerable<(ChatRoom ChatRoom, int UnreadCount, string? LastMessage)>> GetLastMessagesWithUnreadCountForChatRoomsAsync(string userId);
    Task<bool> MarkAllAsReadByChatRoomIdAsync(int chatRoomId, string userId);
    Task<Message> AddMessageAsync(Message message);
    Task<bool> AddUserToChatRoomAsync(string userId, int chatRoomId);
    Task<bool> RemoveUserFromChatRoomAsync(string userId, int chatRoomId);
}
```

Defines data access operations for chat-related entities.

#### IUserRepository

```csharp
public interface IUserRepository
{
    Task<IEnumerable<ApplicationUser>> GetUsersAsync();
    Task<ApplicationUser> GetUserByIdAsync(string userId);
    Task<ApplicationUser> GetUserByUsernameAsync(string username);
    Task<bool> UpdateUserAsync(ApplicationUser user);
}
```

Defines data access operations for user-related entities.

#### IAuthService

```csharp
public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResultDto> LoginAsync(LoginDto loginDto);
}
```

Defines authentication operations.

#### ISentimentAnalysisService

```csharp
public interface ISentimentAnalysisService
{
    Task<(string Score, string Label)> AnalyzeSentimentAsync(string text);
}
```

Defines sentiment analysis operations.

## 4. Infrastructure Layer

### 4.1 Data Context

#### ApplicationDbContext

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }
    public DbSet<MessageRead> MessageReads { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.ChatRoom)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatRoomUser>()
            .HasOne(cu => cu.User)
            .WithMany(u => u.ChatRooms)
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChatRoomUser>()
            .HasOne(cu => cu.ChatRoom)
            .WithMany(c => c.Users)
            .HasForeignKey(cu => cu.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure MessageRead relationships
        modelBuilder.Entity<MessageRead>()
            .HasOne(mr => mr.Message)
            .WithMany(m => m.ReadBy)
            .HasForeignKey(mr => mr.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MessageRead>()
            .HasOne(mr => mr.User)
            .WithMany()
            .HasForeignKey(mr => mr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

Extends Entity Framework's DbContext to define database mappings and relationships.

### 4.2 Repository Implementations

#### ChatRepository

Key implementations:

```csharp
public class ChatRepository : IChatRepository
{
    private readonly ApplicationDbContext _context;

    public ChatRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ChatRoom>> GetChatRoomsAsync(string userId)
    {
        return await _context.ChatRooms
            .Include(c => c.Users)
                .ThenInclude(u => u.User)
            .Where(c => c.Users.Any(u => u.UserId == userId))
            .ToListAsync();
    }

    public async Task<ChatRoom> CreateChatRoomAsync(ChatRoom chatRoom)
    {
        _context.ChatRooms.Add(chatRoom);
        await _context.SaveChangesAsync();
        return chatRoom;
    }

    public async Task<Message> AddMessageAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<bool> MarkAllAsReadByChatRoomIdAsync(int chatRoomId, string userId)
    {
        var messages = await _context.Messages
            .Where(m => m.ChatRoomId == chatRoomId && !_context.MessageReads.Any(r => r.MessageId == m.Id && r.UserId == userId))
            .ToListAsync();

        foreach (var message in messages)
        {
            _context.MessageReads.Add(new MessageRead
            {
                MessageId = message.Id,
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<(ChatRoom ChatRoom, int UnreadCount, string? LastMessage)>> GetLastMessagesWithUnreadCountForChatRoomsAsync(string userId)
    {
        return await _context.ChatRooms
            .Include(c => c.Users)
                .ThenInclude(u => u.User)
            .Where(c => c.Users.Any(u => u.UserId == userId))
            .Select(c => new
            {
                ChatRoom = c,
                UnreadCount = _context.Messages
                    .Where(m => m.ChatRoomId == c.Id && !_context.MessageReads.Any(r => r.MessageId == m.Id && r.UserId == userId))
                    .Count(),
                LastMessage = _context.Messages
                    .Where(m => m.ChatRoomId == c.Id)
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Content)
                    .FirstOrDefault()
            })
            .ToListAsync()
            .ContinueWith(task => task.Result.Select(r => (r.ChatRoom, r.UnreadCount, r.LastMessage)));
    }
}
```

Implements chat-related data access operations defined in IChatRepository.

#### UserRepository

```csharp
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<ApplicationUser> GetUserByIdAsync(string userId)
    {
        return await _context.Users.FindAsync(userId) ??
            throw new InvalidOperationException($"User with ID {userId} not found.");
    }

    public async Task<ApplicationUser> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .SingleOrDefaultAsync(u => u.UserName == username) ??
            throw new InvalidOperationException($"User with username {username} not found.");
    }

    public async Task<bool> UpdateUserAsync(ApplicationUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
        return await _context.SaveChangesAsync() > 0;
    }
}
```

Implements user-related data access operations defined in IUserRepository.

### 4.3 Services

#### SentimentAnalysisService

```csharp
public class SentimentAnalysisService : ISentimentAnalysisService
{
    private readonly TextAnalyticsClient _textAnalyticsClient;

    public SentimentAnalysisService(string azureCognitiveServicesEndpoint, string azureCognitiveServicesKey)
    {
        var credentials = new AzureKeyCredential(azureCognitiveServicesKey);
        _textAnalyticsClient = new TextAnalyticsClient(new Uri(azureCognitiveServicesEndpoint), credentials);
    }

    public async Task<(string Score, string Label)> AnalyzeSentimentAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return ("0", "neutral");

        try
        {
            DocumentSentiment documentSentiment = await _textAnalyticsClient.AnalyzeSentimentAsync(text);
            
            string sentimentLabel = documentSentiment.Sentiment.ToString().ToLower();
            string sentimentScore = GetScoreBasedOnSentiment(documentSentiment);
            
            return (sentimentScore, sentimentLabel);
        }
        catch (Exception)
        {
            // Log exception
            return ("0", "neutral");
        }
    }

    private string GetScoreBasedOnSentiment(DocumentSentiment sentiment)
    {
        return sentiment.Sentiment switch
        {
            TextSentiment.Positive => sentiment.ConfidenceScores.Positive.ToString("0.00"),
            TextSentiment.Negative => (-sentiment.ConfidenceScores.Negative).ToString("0.00"),
            _ => "0.00",
        };
    }
}
```

Implements sentiment analysis using Azure Cognitive Services.

#### AuthService

```csharp
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly string _jwtSecret;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        string jwtSecret)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _jwtSecret = jwtSecret;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
    {
        var user = new ApplicationUser
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            CreatedAt = DateTime.UtcNow,
            LastActive = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await _userManager.AddToRoleAsync(user, "User");

        return await GenerateAuthResultForUserAsync(user);
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.UserName) ??
            throw new InvalidOperationException("User not found");

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Invalid password");
        }

        user.LastActive = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return await GenerateAuthResultForUserAsync(user);
    }

    private async Task<AuthResultDto> GenerateAuthResultForUserAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("name", $"{user.FirstName} {user.LastName}")
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new AuthResultDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName
            },
            Expiration = expires
        };
    }
}
```

Implements user authentication and JWT token generation.

### 4.4 Data Seeding

#### DataSeeder

```csharp
public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DataSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await SeedRoles();
        await SeedUsers();
        await SeedChatRooms();
        await SeedChatRoomUsers();
        await SeedMessages();
        await SeedMessageReads();
    }
}
```

Populates the database with initial test data.

## 5. API Layer

### 5.1 Controllers

#### AuthController

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResultDto>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var result = await _authService.RegisterAsync(registerDto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var result = await _authService.LoginAsync(loginDto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}
```

Handles user registration and login.

#### ChatRoomsController

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChatRoomsController : ControllerBase
{
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;

    public ChatRoomsController(IChatRepository chatRepository, IUserRepository userRepository)
    {
        _chatRepository = chatRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatRoomDto>>> GetChatRooms()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var chatRoomsWithData = await _chatRepository.GetLastMessagesWithUnreadCountForChatRoomsAsync(userId);
        
        return Ok(chatRoomsWithData.Select(c => new ChatRoomDto
        {
            Id = c.ChatRoom.Id,
            Name = c.ChatRoom.Name,
            CreatedAt = c.ChatRoom.CreatedAt,
            Users = c.ChatRoom.Users.Select(u => new UserDto
            {
                Id = u.User.Id,
                UserName = u.User.UserName!,
                Email = u.User.Email!,
                FirstName = u.User.FirstName,
                LastName = u.User.LastName
            }).ToList(),
            UnreadCount = c.UnreadCount,
            LastMessage = c.LastMessage,
            MessageCount = c.ChatRoom.Messages.Count
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChatRoomDto>> GetChatRoom(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(id);
            
            // Check if user is a member of the chat room
            if (!chatRoom.Users.Any(u => u.UserId == userId))
            {
                return Forbid();
            }
            
            var chatRoomData = await _chatRepository.GetLastMessagesWithUnreadCountForChatRoomsAsync(userId);
            var chatRoomWithData = chatRoomData.FirstOrDefault(c => c.ChatRoom.Id == id);
            
            return Ok(new ChatRoomDto
            {
                Id = chatRoom.Id,
                Name = chatRoom.Name,
                CreatedAt = chatRoom.CreatedAt,
                Users = chatRoom.Users.Select(u => new UserDto
                {
                    Id = u.User.Id,
                    UserName = u.User.UserName!,
                    Email = u.User.Email!,
                    FirstName = u.User.FirstName,
                    LastName = u.User.LastName
                }).ToList(),
                UnreadCount = chatRoomWithData?.UnreadCount ?? 0,
                LastMessage = chatRoomWithData?.LastMessage,
                MessageCount = chatRoom.Messages.Count
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ChatRoomDto>> CreateChatRoom(CreateChatRoomDto createChatRoomDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        var chatRoom = new ChatRoom
        {
            Name = createChatRoomDto.Name,
            CreatedAt = DateTime.UtcNow,
            Users = new List<ChatRoomUser>
            {
                new ChatRoomUser
                {
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                }
            }
        };
        
        if (createChatRoomDto.UserIds != null)
        {
            foreach (var id in createChatRoomDto.UserIds.Where(id => id != userId))
            {
                chatRoom.Users.Add(new ChatRoomUser
                {
                    UserId = id,
                    JoinedAt = DateTime.UtcNow
                });
            }
        }

        var createdChatRoom = await _chatRepository.CreateChatRoomAsync(chatRoom);

        var chatRoomDto = new ChatRoomDto
        {
            Id = createdChatRoom.Id,
            Name = createdChatRoom.Name,
            CreatedAt = createdChatRoom.CreatedAt,
            Users = createdChatRoom.Users.Select(u => new UserDto
            {
                Id = u.UserId,
                UserName = u.User.UserName!,
                Email = u.User.Email!,
                FirstName = u.User.FirstName,
                LastName = u.User.LastName
            }).ToList(),
            MessageCount = 0
        };

        return CreatedAtAction(nameof(GetChatRoom), new { id = chatRoom.Id }, chatRoomDto);
    }

    [HttpPost("{id}/read")]
    public async Task<ActionResult> MarkAllAsRead(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        try
        {
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(id);
            
            // Check if user is a member of the chat room
            if (!chatRoom.Users.Any(u => u.UserId == userId))
            {
                return Forbid();
            }
            
            var result = await _chatRepository.MarkAllAsReadByChatRoomIdAsync(id, userId);
            
            if (result)
            {
                return Ok();
            }
            
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
```

Handles chat room operations like creating, retrieving, and marking messages as read.

#### MessagesController

```csharp
[ApiController]
[Route("api/chatrooms/{chatRoomId}/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MessagesController : ControllerBase
{
    private readonly IChatRepository _chatRepository;

    public MessagesController(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(int chatRoomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        try
        {
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(chatRoomId);
            
            // Check if user is a member of the chat room
            if (!chatRoom.Users.Any(u => u.UserId == userId))
            {
                return Forbid();
            }
            
            var messagesWithStatus = await _chatRepository.GetMessagesForChatRoomWithStatusAsync(chatRoomId, userId, page, pageSize);
            
            var messageDtos = messagesWithStatus.Select(m => new MessageDto
            {
                Id = m.Message.Id,
                Content = m.Message.Content,
                SentAt = m.Message.SentAt,
                SenderUserName = m.Message.Sender.UserName!,
                SenderFullName = $"{m.Message.Sender.FirstName} {m.Message.Sender.LastName}",
                ChatRoomId = m.Message.ChatRoomId!.Value,
                SentimentLabel = m.Message.SentimentLabel,
                IsRead = m.IsRead
            }).ToList();
            
            return Ok(messageDtos);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
```

Handles message retrieval operations.

#### UsersController

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userRepository.GetUsersAsync();
        
        return Ok(users.Select(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName!,
            Email = u.Email!,
            FirstName = u.FirstName,
            LastName = u.LastName
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            
            return Ok(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
```

Handles user retrieval operations.

### 5.2 SignalR Hubs

#### ChatHub

```csharp
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChatHub : Hub
{
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISentimentAnalysisService _sentimentAnalysisService;
    private readonly ILogger<ChatHub> _logger;
    
    // Thread-safe dictionary to keep track of user connections and their groups
    private static readonly ConcurrentDictionary<string, HashSet<string>> _connectionGroups = new();
    private static readonly ConcurrentDictionary<string, string> _userConnections = new();

    public ChatHub(
        IChatRepository chatRepository,
        IUserRepository userRepository,
        ISentimentAnalysisService sentimentAnalysisService,
        ILogger<ChatHub> logger)
    {
        _chatRepository = chatRepository;
        _userRepository = userRepository;
        _sentimentAnalysisService = sentimentAnalysisService;
        _logger = logger;
    }

    // Connection management
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var connectionId = Context.ConnectionId;
        
        _userConnections.AddOrUpdate(userId, connectionId, (_, _) => connectionId);
        _connectionGroups.TryAdd(connectionId, new HashSet<string>());
        
        var user = await _userRepository.GetUserByIdAsync(userId);
        user.LastActive = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);
        
        _logger.LogInformation($"User {userId} connected with connection ID {connectionId}");
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var connectionId = Context.ConnectionId;
        
        _connectionGroups.TryRemove(connectionId, out _);
        
        if (_userConnections.TryGetValue(userId, out var storedConnectionId) && storedConnectionId == connectionId)
        {
            _userConnections.TryRemove(userId, out _);
        }
        
        _logger.LogInformation($"User {userId} disconnected");
        
        await base.OnDisconnectedAsync(exception);
    }

    // Chat room operations
    public async Task JoinChatRoom(int chatRoomId)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var connectionId = Context.ConnectionId;
        string groupName = $"ChatRoom_{chatRoomId}";
        
        try
        {
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(chatRoomId);
            
            if (!chatRoom.Users.Any(u => u.UserId == userId))
            {
                _logger.LogWarning($"User {userId} is not authorized to join chat room {chatRoomId}");
                await Clients.Caller.SendAsync("Error", "You are not authorized to join this chat room.");
                return;
            }
            
            _connectionGroups.AddOrUpdate(connectionId, new HashSet<string> { groupName }, (_, existingGroups) =>
                {
                    existingGroups.Add(groupName);
                    return existingGroups;
                });
            
            await Groups.AddToGroupAsync(connectionId, groupName);
            _logger.LogInformation($"User {userId} joined chat room {chatRoomId}");
            
            // Send recent messages to the client
            var recentMessages = await _chatRepository.GetMessagesForChatRoomWithStatusAsync(chatRoomId, userId);
            
            if (recentMessages != null && recentMessages.Any())
            {
                var messageDtos = new List<MessageDto>();
                
                foreach (var message in recentMessages)
                {
                    var sender = await _userRepository.GetUserByIdAsync(message.Message.SenderId);
                    
                    messageDtos.Add(new MessageDto
                    {
                        Id = message.Message.Id,
                        Content = message.Message.Content,
                        SentAt = message.Message.SentAt,
                        SenderUserName = sender.UserName!,
                        SenderFullName = $"{sender.FirstName} {sender.LastName}",
                        ChatRoomId = message.Message.ChatRoomId!.Value,
                        SentimentLabel = message.Message.SentimentLabel,
                        IsRead = message.IsRead
                    });
                }
                
                _logger.LogInformation($"Sending {messageDtos.Count} messages to user {userId}");
                await Clients.Caller.SendAsync("LoadMessages", messageDtos);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in JoinChatRoom: {ex.Message}");
            await Clients.Caller.SendAsync("Error", $"Failed to join chat room: {ex.Message}");
        }
    }

    public async Task SendMessage(CreateMessageDto messageDto)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var connectionId = Context.ConnectionId;
        string groupName = $"ChatRoom_{messageDto.ChatRoomId}";
        
        try
        {
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(messageDto.ChatRoomId);
            
            if (!chatRoom.Users.Any(u => u.UserId == userId))
            {
                _logger.LogWarning($"User {userId} is not authorized to send messages to chat room {messageDto.ChatRoomId}");
                await Clients.Caller.SendAsync("Error", "You are not authorized to send messages to this chat room.");
                return;
            }
            
            var user = await _userRepository.GetUserByIdAsync(userId);
            
            // Make sure the connection is in the group
            if (_connectionGroups.TryGetValue(connectionId, out var groups) && !groups.Contains(groupName))
            {
                _connectionGroups.AddOrUpdate(connectionId, new HashSet<string> { groupName }, (_, existingGroups) =>
                    {
                        existingGroups.Add(groupName);
                        return existingGroups;
                    });
                
                await Groups.AddToGroupAsync(connectionId, groupName);
            }

            var (sentimentScore, sentimentLabel) = await _sentimentAnalysisService.AnalyzeSentimentAsync(messageDto.Content);

            var message = new Message
            {
                Content = messageDto.Content,
                SentAt = DateTime.UtcNow,
                SenderId = userId,
                ChatRoomId = messageDto.ChatRoomId,
                SentimentScore = sentimentScore,
                SentimentLabel = sentimentLabel
            };

            await _chatRepository.AddMessageAsync(message);

            var messageToReturn = new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                SenderUserName = user.UserName!,
                SenderFullName = $"{user.FirstName} {user.LastName}",
                ChatRoomId = message.ChatRoomId.Value,
                SentimentLabel = message.SentimentLabel
            };

            _logger.LogInformation($"Broadcasting message to group {groupName}");
            
            await Clients.Group(groupName).SendAsync("ReceiveMessage", messageToReturn);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in SendMessage: {ex.Message}");
            await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
        }
    }

    public async Task SendTypingNotification(int chatRoomId)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        string groupName = $"ChatRoom_{chatRoomId}";
        
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync("UserTyping", new
            {
                UserId = userId,
                UserName = user.UserName,
                FullName = $"{user.FirstName} {user.LastName}",
                ChatRoomId = chatRoomId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in SendTypingNotification: {ex.Message}");
        }
    }

    public async Task MarkAsRead(int messageId)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        try
        {
            var message = await _context.Messages.FindAsync(messageId);
            
            if (message == null)
            {
                return;
            }
            
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(message.ChatRoomId!.Value);
            
            if (!chatRoom.Users.Any(u => u.UserId == userId))
            {
                return;
            }
            
            var messageRead = new MessageRead
            {
                MessageId = messageId,
                UserId = userId,
                ReadAt = DateTime.UtcNow
            };
            
            _context.MessageReads.Add(messageRead);
            await _context.SaveChangesAsync();
            
            string groupName = $"ChatRoom_{message.ChatRoomId!.Value}";
            await Clients.Group(groupName).SendAsync("MessageRead", new
            {
                MessageId = messageId,
                UserId = userId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in MarkAsRead: {ex.Message}");
        }
    }

    public async Task GetConnectionInfo()
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var connectionId = Context.ConnectionId;
        var chatRooms = await _chatRepository.GetChatRoomsAsync(userId);
        
        var activeGroups = new List<string>();
        if (_connectionGroups.TryGetValue(connectionId, out var groups))
        {
            activeGroups = groups.ToList();
        }
        
        await Clients.Caller.SendAsync("ConnectionInfo", new { 
            connectionId = connectionId,
            userId = userId,
            availableChatRooms = chatRooms.Select(cr => cr.Id).ToList(),
            activeGroups = activeGroups
        });
    }

    public async Task Heartbeat()
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var connectionId = Context.ConnectionId;
        
        // Re-establish group membership if needed
        if (_connectionGroups.TryGetValue(connectionId, out var groups))
        {
            foreach (var group in groups)
            {
                await Groups.AddToGroupAsync(connectionId, group);
            }
        }
        
        await Clients.Caller.SendAsync("Heartbeat", new { timestamp = DateTime.UtcNow });
    }
}
```

Handles real-time chat communication.

### 5.3 Authentication & Authorization

Authentication is managed through the ASP.NET Core Identity system, integrated with JWT bearer authentication for API and SignalR hub authorization.

JWT Configuration in Program.cs:

```csharp
// Configure JWT Authentication with industry best practices
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Ensure token is signed with our secret key
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Env.GetString("JWT_TOKEN_SECRET"))),
        
        // Disable issuer and audience validation for simple token auth
        ValidateIssuer = false,
        ValidateAudience = false,
        
        // Configure token lifetime validation
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    // Configure JWT handler for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Allow JWT authentication for SignalR connections
            var accessToken = context.Request.Query["access_token"];
            
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
```

Identity Configuration:
```csharp
// Configure ASP.NET Core Identity with secure password requirements
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Security best practices for password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

## 6. Azure Integration

### 6.1 Azure SQL Database

The application is configured to use Azure SQL Database for data storage.

```csharp
// Configure Entity Framework to use Azure SQL with appropriate connection pooling
// Connection string is stored in environment variables for security
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(Env.GetString("SQL_CONNECTION_STRING")), 
    ServiceLifetime.Scoped);
```

#### Azure SQL Database Best Practices

- **Connection Pooling**: Enabled by default in EF Core
- **Scoped Lifetime**: Ensures proper connection management
- **Connection Resilience**: Can be added with EnableRetryOnFailure() option
- **Query Optimization**: Use indexes for frequently queried columns

### 6.2 Azure SignalR Service

Real-time communication is powered by Azure SignalR Service:

```csharp
// Configure Azure SignalR Service for scalable real-time communication
// Uses managed service to handle connection scaling and WebSocket infrastructure
builder.Services.AddSignalR()
    .AddAzureSignalR(options =>
    {
        options.ConnectionString = Env.GetString("AZURE_SIGNALR_CONNECTION_STRING");
        
        // Initial hub connections to optimize performance under load
        // Helps reduce connection latency for the first set of clients
        options.InitialHubServerConnectionCount = 10;
        
        // Sets the lifetime for access tokens used in client-service authentication
        // Balances security (shorter lifetime) with user experience (avoiding frequent reconnects)
        options.AccessTokenLifetime = TimeSpan.FromHours(1);
    });
```

#### Azure SignalR Best Practices

- **Connection Management**: Use a sufficient number of initial connections
- **Server-side Scaling**: Azure SignalR handles connection scaling automatically
- **Client Reconnection**: Implement client-side reconnection logic
- **Access Token Lifetime**: Balance security with user experience

### 6.3 Azure Cognitive Services

Sentiment analysis is performed using Azure Cognitive Services:

```csharp
// Register Azure Cognitive Services for text sentiment analysis
builder.Services.AddScoped<ISentimentAnalysisService>(provider =>
    new SentimentAnalysisService(
        Env.GetString("COGNITIVE_SERVICE_ENDPOINT"),
        Env.GetString("COGNITIVE_SERVICE_KEY_1")));
```

#### Azure Cognitive Services Best Practices

- **Error Handling**: Implement robust error handling for API calls
- **Rate Limiting**: Be aware of service limits and implement throttling
- **Caching**: Consider caching sentiment results for repeated phrases
- **Key Management**: Rotate keys periodically and use Key Vault in production

### 6.4 Connection String Management

Connection strings are managed using environment variables loaded from a .env file for local development and Azure configuration settings for production.

```csharp
// Load environment variables from .env file for local development and testing
// In production, these variables should be set in Azure App Configuration
Env.Load();
```

## 7. Real-time Communication

### 7.1 SignalR Implementation

SignalR provides real-time, bi-directional communication between the server and clients. The implementation includes:

- **User authentication**: JWT bearer authentication for hub connections
- **Group management**: Chat rooms are represented as SignalR groups
- **Connection tracking**: Concurrent dictionaries track user connections and group memberships
- **Connection resilience**: Heartbeat mechanism to maintain connections

### 7.2 Connection Management

```csharp
// Thread-safe dictionary to keep track of user connections and their groups
private static readonly ConcurrentDictionary<string, HashSet<string>> _connectionGroups = new();
private static readonly ConcurrentDictionary<string, string> _userConnections = new();
```

The hub tracks connections using thread-safe dictionaries:
- `_connectionGroups`: Maps connection IDs to the groups (chat rooms) they've joined
- `_userConnections`: Maps user IDs to their current connection ID

### 7.3 Message Broadcasting

```csharp
public async Task SendMessage(CreateMessageDto messageDto)
{
    // ... validation and processing ...
    
    await Clients.Group(groupName).SendAsync("ReceiveMessage", messageToReturn);
}
```

Messages are broadcast to all clients in the chat room group, including:
- Message content and metadata
- Sender information
- Sentiment analysis results

## 8. Sentiment Analysis

### 8.1 Analysis Process

The application analyzes the sentiment of each message using Azure Cognitive Services Text Analytics:

```csharp
public async Task<(string Score, string Label)> AnalyzeSentimentAsync(string text)
{
    if (string.IsNullOrEmpty(text))
        return ("0", "neutral");

    try
    {
        DocumentSentiment documentSentiment = await _textAnalyticsClient.AnalyzeSentimentAsync(text);
        
        string sentimentLabel = documentSentiment.Sentiment.ToString().ToLower();
        string sentimentScore = GetScoreBasedOnSentiment(documentSentiment);
        
        return (sentimentScore, sentimentLabel);
    }
    catch (Exception)
    {
        // Log exception
        return ("0", "neutral");
    }
}
```

### 8.2 Score Calculation

The sentiment score is calculated based on the confidence scores returned by Azure:

```csharp
private string GetScoreBasedOnSentiment(DocumentSentiment sentiment)
{
    return sentiment.Sentiment switch
    {
        TextSentiment.Positive => sentiment.ConfidenceScores.Positive.ToString("0.00"),
        TextSentiment.Negative => (-sentiment.ConfidenceScores.Negative).ToString("0.00"),
        _ => "0.00",
    };
}
```

- Positive sentiments get a positive score (0 to 1)
- Negative sentiments get a negative score (-1 to 0)
- Neutral sentiments get a score of 0

## 9. Database Schema

### 9.1 Entity Relationships

The application's database schema consists of these primary relationships:

- One-to-Many: User to Messages
- One-to-Many: ChatRoom to Messages
- Many-to-Many: User to ChatRooms (via ChatRoomUser)
- One-to-Many: Message to MessageReads

Entity Framework Core fluent API is used to configure these relationships:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Configure relationships
    modelBuilder.Entity<Message>()
        .HasOne(m => m.Sender)
        .WithMany(u => u.Messages)
        .HasForeignKey(m => m.SenderId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Message>()
        .HasOne(m => m.ChatRoom)
        .WithMany(c => c.Messages)
        .HasForeignKey(m => m.ChatRoomId)
        .OnDelete(DeleteBehavior.Cascade);

    // Additional relationship configurations...
}
```

### 9.2 Indexes

The database schema includes indexes for frequently queried columns:

- Primary keys on all tables
- Foreign keys for relationships
- ASP.NET Identity's built-in indexes for user lookups

### 9.3 Migration Strategy

The project uses Entity Framework Core migrations to manage database schema changes:

1. Create initial migration to establish schema
2. Apply migrations automatically during deployment
3. Use migration bundles for production deployments
4. Run data seeding separately in development environments

## 10. Project Setup

### 10.1 Prerequisites

- .NET 8.0 SDK
- SQL Server or Azure SQL Database
- Azure SignalR Service instance
- Azure Cognitive Services Text Analytics resource
- Visual Studio or VS Code with C# extension

### 10.2 Environment Configuration

Create a `.env` file in the ReenbitTest.API directory with the following variables:

```
SQL_CONNECTION_STRING=<your-sql-connection-string>
JWT_TOKEN_SECRET=<your-jwt-secret>
AZURE_SIGNALR_CONNECTION_STRING=<your-signalr-connection-string>
COGNITIVE_SERVICE_ENDPOINT=<your-cognitive-services-endpoint>
COGNITIVE_SERVICE_KEY_1=<your-cognitive-services-key>
```

For production, configure these settings in Azure App Configuration or Azure App Service Configuration.

### 10.3 Database Setup

First-time setup:

```bash
# Navigate to the solution directory
cd /path/to/API
# Create a migration
dotnet ef migrations add MigrationName --project ReenbitTest.Infrastructure --startup-project ReenbitTest.API
# Apply migrations
dotnet ef database update --project ReenbitTest.Infrastructure --startup-project ReenbitTest.API
```

### 10.4 Running the Application

```bash
# Navigate to the API directory
cd /path/to/API/ReenbitTest.API
# Run the application
dotnet run
```

The API will be available at:
- https://localhost:5001 (HTTPS)
- http://localhost:5000 (HTTP)

## 11. API Documentation

### 11.1 Authentication Endpoints

#### Register a new user

**POST /api/Auth/register**

Request Body:
```json
{
  "userName": "john.doe@example.com",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

Response Body:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "1234567890",
    "userName": "john.doe@example.com",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe"
  },
  "expiration": "2023-07-01T00:00:00Z"
}
```

#### Login

**POST /api/Auth/login**

Request Body:
```json
{
  "userName": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

Response Body:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "1234567890",
    "userName": "john.doe@example.com",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe"
  },
  "expiration": "2023-07-01T00:00:00Z"
}
```

### 11.2 User Endpoints

#### Get all users

**GET /api/Users**

Authentication: Required (JWT Bearer Token)

Response Body:
```json
[
  {
    "id": "1234567890",
    "userName": "john.doe@example.com",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe"
  },
  {
    "id": "0987654321",
    "userName": "jane.smith@example.com",
    "email": "jane.smith@example.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "fullName": "Jane Smith"
  }
]
```

#### Get user by ID

**GET /api/Users/{id}**

Authentication: Required (JWT Bearer Token)

Response Body:
```json
{
  "id": "1234567890",
  "userName": "john.doe@example.com",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "fullName": "John Doe"
}
```

### 11.3 Chat Room Endpoints

#### Get all chat rooms for current user

**GET /api/ChatRooms**

Authentication: Required (JWT Bearer Token)

Response Body:
```json
[
  {
    "id": 1,
    "name": "General Chat",
    "createdAt": "2023-06-01T12:00:00Z",
    "users": [
      {
        "id": "1234567890",
        "userName": "john.doe@example.com",
        "email": "john.doe@example.com",
        "firstName": "John",
        "lastName": "Doe",
        "fullName": "John Doe"
      }
    ],
    "messageCount": 10,
    "unreadCount": 2,
    "lastMessage": "Hello, world!"
  }
]
```

#### Get specific chat room

**GET /api/ChatRooms/{id}**

Authentication: Required (JWT Bearer Token)

Response Body:
```json
{
  "id": 1,
  "name": "General Chat",
  "createdAt": "2023-06-01T12:00:00Z",
  "users": [
    {
      "id": "1234567890",
      "userName": "john.doe@example.com",
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "fullName": "John Doe"
    }
  ],
  "messageCount": 10,
  "unreadCount": 2,
  "lastMessage": "Hello, world!"
}
```

#### Create a new chat room

**POST /api/ChatRooms**

Authentication: Required (JWT Bearer Token)

Request Body:
```json
{
  "name": "New Chat Room",
  "userIds": ["1234567890", "0987654321"]
}
```

Response Body:
```json
{
  "id": 2,
  "name": "New Chat Room",
  "createdAt": "2023-06-15T12:00:00Z",
  "users": [
    {
      "id": "1234567890",
      "userName": "john.doe@example.com",
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "fullName": "John Doe"
    },
    {
      "id": "0987654321",
      "userName": "jane.smith@example.com",
      "email": "jane.smith@example.com",
      "firstName": "Jane",
      "lastName": "Smith",
      "fullName": "Jane Smith"
    }
  ],
  "messageCount": 0,
  "unreadCount": 0,
  "lastMessage": null
}
```

#### Mark all messages in a chat room as read

**POST /api/ChatRooms/{id}/read**

Authentication: Required (JWT Bearer Token)

Response: HTTP 200 OK

### 11.4 Message Endpoints

Continuing with the detailed documentation file:


#### Get messages from a chat room

**GET /api/chatrooms/{chatRoomId}/Messages**

Authentication: Required (JWT Bearer Token)

Query Parameters:
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Number of messages per page (default: 20)

Response Body:
```json
[
  {
    "id": 1,
    "content": "Hello, everyone!",
    "sentAt": "2023-06-15T12:05:00Z",
    "senderUserName": "john.doe@example.com",
    "senderFullName": "John Doe",
    "chatRoomId": 1,
    "sentimentLabel": "positive",
    "isRead": true
  },
  {
    "id": 2,
    "content": "Welcome to the chat!",
    "sentAt": "2023-06-15T12:06:00Z",
    "senderUserName": "jane.smith@example.com",
    "senderFullName": "Jane Smith",
    "chatRoomId": 1,
    "sentimentLabel": "positive",
    "isRead": false
  }
]
```

## 12. Deployment

### 12.1 Azure Deployment

The application is designed for deployment to Azure App Service with Azure SQL Database.

Azure resources required:
- Azure App Service Plan
- Azure App Service
- Azure SQL Database
- Azure SignalR Service
- Azure Cognitive Services Text Analytics

### 12.2 CI/CD Pipeline

The solution includes GitHub Actions workflows for continuous integration and deployment:

```yaml
# .github/workflows/build-deploy.yml
name: Build and Deploy

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Test
      run: dotnet test --no-build --verbosity normal --configuration Release
      
    - name: Publish
      run: dotnet publish --no-build --configuration Release --output ./publish
      
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: app
        path: ./publish

  deploy:
    needs: build
    if: github.event_name != 'pull_request'
    runs-on: ubuntu-latest
    
    steps:
    - name: Download artifact
      uses: actions/download-artifact@v3
      with:
        name: app
        path: ./app
        
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: reenbittest
        publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
        package: ./app
```

## 13. Common Development Tasks

### 13.1 Adding a New Entity

1. **Create the Entity Class**:
   ```csharp
   public class NewEntity
   {
       public int Id { get; set; }
       public string Name { get; set; } = null!;
       // Other properties
   }
   ```

2. **Add to DbContext**:
   ```csharp
   public DbSet<NewEntity> NewEntities { get; set; }
   ```

3. **Configure relationships in OnModelCreating**:
   ```csharp
   modelBuilder.Entity<NewEntity>()
       .HasOne(ne => ne.RelatedEntity)
       .WithMany(re => re.NewEntities)
       .HasForeignKey(ne => ne.RelatedEntityId);
   ```

4. **Create a repository interface**:
   ```csharp
   public interface INewEntityRepository
   {
       Task<IEnumerable<NewEntity>> GetAllAsync();
       Task<NewEntity> GetByIdAsync(int id);
       Task<NewEntity> AddAsync(NewEntity entity);
       Task<bool> UpdateAsync(NewEntity entity);
       Task<bool> DeleteAsync(int id);
   }
   ```

5. **Implement the repository**:
   ```csharp
   public class NewEntityRepository : INewEntityRepository
   {
       private readonly ApplicationDbContext _context;
       
       public NewEntityRepository(ApplicationDbContext context)
       {
           _context = context;
       }
       
       // Implement interface methods
   }
   ```

6. **Create DTO classes**:
   ```csharp
   public class NewEntityDto
   {
       public int Id { get; set; }
       public string Name { get; set; } = null!;
       // Other properties
   }
   ```

7. **Register the repository in Program.cs**:
   ```csharp
   builder.Services.AddScoped<INewEntityRepository, NewEntityRepository>();
   ```

8. **Create a controller**:
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
   public class NewEntitiesController : ControllerBase
   {
       private readonly INewEntityRepository _repository;
       
       public NewEntitiesController(INewEntityRepository repository)
       {
           _repository = repository;
       }
       
       // Implement controller methods
   }
   ```

9. **Create a migration**:
   ```bash
   dotnet ef migrations add AddNewEntity --project ReenbitTest.Infrastructure --startup-project ReenbitTest.API
   ```

10. **Apply the migration**:
    ```bash
    dotnet ef database update --project ReenbitTest.Infrastructure --startup-project ReenbitTest.API
    ```

### 13.2 Creating a Migration

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project ReenbitTest.Infrastructure --startup-project ReenbitTest.API

# Apply migrations to the database
dotnet ef database update --project ReenbitTest.Infrastructure --startup-project ReenbitTest.API

# Generate SQL script for a migration (useful for production deployments)
dotnet ef migrations script PreviousMigration MigrationName --project ReenbitTest.Infrastructure --startup-project ReenbitTest.API --output migration.sql
```

### 13.3 Adding a New API Endpoint

1. **Create/update a DTO** (if needed):
   ```csharp
   public class NewRequestDto
   {
       public string Property1 { get; set; } = null!;
       public int Property2 { get; set; }
   }
   
   public class NewResponseDto
   {
       public int Id { get; set; }
       public string Property1 { get; set; } = null!;
       public int Property2 { get; set; }
       public DateTime CreatedAt { get; set; }
   }
   ```

2. **Add a method to the appropriate repository interface**:
   ```csharp
   Task<Entity> ProcessNewOperationAsync(string param1, int param2);
   ```

3. **Implement the repository method**:
   ```csharp
   public async Task<Entity> ProcessNewOperationAsync(string param1, int param2)
   {
       // Implementation
   }
   ```

4. **Add the endpoint to the controller**:
   ```csharp
   [HttpPost("newOperation")]
   public async Task<ActionResult<NewResponseDto>> NewOperation([FromBody] NewRequestDto request)
   {
       try
       {
           var result = await _repository.ProcessNewOperationAsync(request.Property1, request.Property2);
           
           var response = new NewResponseDto
           {
               Id = result.Id,
               Property1 = result.Property1,
               Property2 = result.Property2,
               CreatedAt = result.CreatedAt
           };
           
           return Ok(response);
       }
       catch (InvalidOperationException ex)
       {
           return BadRequest(ex.Message);
       }
   }
   ```