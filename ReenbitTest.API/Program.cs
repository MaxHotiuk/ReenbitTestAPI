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

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
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

// Configure DbContext with Azure SQL
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(Env.GetString("SQL_CONNECTION_STRING")), 
    ServiceLifetime.Scoped);

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Env.GetString("JWT_TOKEN_SECRET"))),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Configure for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // More permissive than AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure SignalR
builder.Services.AddSignalR()
    .AddAzureSignalR(options =>
    {
        options.ConnectionString = Env.GetString("SIGNALR_CONNECTION_STRING");

        // Important settings for connection stability
        options.ServerStickyMode = ServerStickyMode.Required;
        options.GracefulShutdown.Mode = GracefulShutdownMode.WaitForClientsClose;
        options.GracefulShutdown.Timeout = TimeSpan.FromSeconds(30);

        options.InitialHubServerConnectionCount = 10;
        
        // This helps track events like connection issues
        options.AccessTokenLifetime = TimeSpan.FromHours(1);
    });

// Register repositories and services
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IAuthService>(provider => 
    new AuthService(
        provider.GetRequiredService<UserManager<ApplicationUser>>(),
        provider.GetRequiredService<SignInManager<ApplicationUser>>(),
        provider.GetRequiredService<IConfiguration>(),
        Env.GetString("JWT_TOKEN_SECRET")));

builder.Services.AddScoped<ISentimentAnalysisService>(provider =>
    new SentimentAnalysisService(
        Env.GetString("COGNITIVE_SERVICE_ENDPOINT"),
        Env.GetString("COGNITIVE_SERVICE_KEY_1")));

builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<DataSeeder>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seeder = services.GetRequiredService<DataSeeder>();
        // await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();