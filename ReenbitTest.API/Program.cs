using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReenbitTest.API.Hubs;
using ReenbitTest.Core.Entities;
using ReenbitTest.Core.Interfaces;
using ReenbitTest.Infrastructure.Data;
using ReenbitTest.Infrastructure.Repositories;
using ReenbitTest.Infrastructure.Services;
using System.Text;
using DotNetEnv;
using Microsoft.Azure.SignalR;

// -------------------------------------------------------------------------
// Environment Setup
// -------------------------------------------------------------------------
// Load environment variables from .env file for local development and testing
// In production, these variables should be set in Azure App Configuration
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------------------
// API Documentation & Developer Tools
// -------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Setup Swagger with JWT authentication support for API documentation and testing
builder.Services.AddSwaggerGen(options =>
{
    // Configure JWT bearer authentication for Swagger UI
    // This allows developers to authenticate and test secured endpoints
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// -------------------------------------------------------------------------
// Azure SQL Database Configuration
// -------------------------------------------------------------------------
// Configure Entity Framework to use Azure SQL with appropriate connection pooling
// Connection string is stored in environment variables for security
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(Env.GetString("SQL_CONNECTION_STRING")), 
    // Scoped lifetime ensures proper connection management per request
    ServiceLifetime.Scoped);

// -------------------------------------------------------------------------
// Identity & Authentication Configuration
// -------------------------------------------------------------------------
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
        
        // Validate issuer and audience to prevent token misuse
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        
        // Check token expiration and set zero clock skew for precise timing
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Special configuration for SignalR to extract token from query string
    // This enables authentication in SignalR WebSocket connections
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            // Only extract token for SignalR hub requests
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

// -------------------------------------------------------------------------
// CORS Configuration
// -------------------------------------------------------------------------
// Configure Cross-Origin Resource Sharing to allow frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development environment: more permissive CORS policy
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // Required for SignalR
        }
        else
        {
            // Production environment: restrict to specific origins
            // Get origins from configuration - handle both string and string[] formats
            var originsConfig = builder.Configuration.GetSection("AllowedOrigins");
            string[] allowedOrigins;
            
            // Check if it's a string or array and process accordingly
            if (originsConfig.Value != null)
            {
                // It's a single string value
                allowedOrigins = new string[] { originsConfig.Value };
            }
            else
            {
                // It's an array
                allowedOrigins = originsConfig.Get<string[]>() ?? new string[] 
                { 
                    "https://yellow-river-0e652990f.6.azurestaticapps.net"
                };
            }
            
            // Apply the origins - ensure they're not empty
            if (allowedOrigins.Length == 0)
            {
                allowedOrigins = new string[] { "https://yellow-river-0e652990f.6.azurestaticapps.net" };
            }
            
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // Required for SignalR
        }
    });
});

// -------------------------------------------------------------------------
// Azure SignalR Service Configuration
// -------------------------------------------------------------------------
// Configure Azure SignalR Service for scalable real-time messaging
builder.Services.AddSignalR()
    .AddAzureSignalR(options =>
    {
        // Connection string from environment variables
        options.ConnectionString = Env.GetString("SIGNALR_CONNECTION_STRING");

        // Server sticky mode ensures consistent routing for client connections
        // Required for features like user-to-user messaging and connection reliability
        options.ServerStickyMode = ServerStickyMode.Required;
        
        // Graceful shutdown configuration ensures active connections are not abruptly terminated
        // during app service restarts or deployments
        options.GracefulShutdown.Mode = GracefulShutdownMode.WaitForClientsClose;
        options.GracefulShutdown.Timeout = TimeSpan.FromSeconds(30);

        // Initial hub connections to optimize performance under load
        // Helps reduce connection latency for the first set of clients
        options.InitialHubServerConnectionCount = 10;
        
        // Sets the lifetime for access tokens used in client-service authentication
        // Balances security (shorter lifetime) with user experience (avoiding frequent reconnects)
        options.AccessTokenLifetime = TimeSpan.FromHours(1);
    });

// -------------------------------------------------------------------------
// DI Container Service Registration
// -------------------------------------------------------------------------
// Register application services with the dependency injection container
// Repositories and services are registered with scoped lifetime for proper resource management

// Register authentication service with JWT token support
builder.Services.AddScoped<IAuthService>(provider => 
    new AuthService(
        provider.GetRequiredService<UserManager<ApplicationUser>>(),
        provider.GetRequiredService<SignInManager<ApplicationUser>>(),
        provider.GetRequiredService<IConfiguration>(),
        Env.GetString("JWT_TOKEN_SECRET")));

// Register Azure Cognitive Services for text sentiment analysis
builder.Services.AddScoped<ISentimentAnalysisService>(provider =>
    new SentimentAnalysisService(
        Env.GetString("COGNITIVE_SERVICE_ENDPOINT"),
        Env.GetString("COGNITIVE_SERVICE_KEY_1")));

// Register data access repositories
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register data seeding service for initialization
builder.Services.AddScoped<DataSeeder>();

// -------------------------------------------------------------------------
// Application Pipeline Configuration
// -------------------------------------------------------------------------
var app = builder.Build();

// Configure the HTTP request pipeline with appropriate middleware
if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI in development environment only
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -------------------------------------------------------------------------
// Database Seeding
// -------------------------------------------------------------------------
// Seed initial data for testing and development
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seeder = services.GetRequiredService<DataSeeder>();
        // Uncomment to seed data during startup
        // await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// -------------------------------------------------------------------------
// Application Middleware Pipeline
// -------------------------------------------------------------------------
// Enforce HTTPS for all communications
app.UseHttpsRedirection();

// Apply CORS policy to allow frontend access
app.UseCors("CorsPolicy");

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map API controllers and SignalR hub
app.MapControllers();
app.MapHub<ChatHub>("/chatHub"); // Maps the chat hub to the /chatHub endpoint

// Start the application
app.Run();