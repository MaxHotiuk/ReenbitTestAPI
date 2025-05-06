using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ReenbitTest.Core.DTOs;
using ReenbitTest.Core.Entities;
using ReenbitTest.Core.Interfaces;

namespace ReenbitTest.Infrastructure.Services
{
    /// <summary>
    /// Implementation of <see cref="IAuthService"/> that handles authentication using ASP.NET Core Identity
    /// and JWT token generation.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecret;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="userManager">The user manager for managing user accounts.</param>
        /// <param name="signInManager">The sign-in manager for handling user sign-in operations.</param>
        /// <param name="configuration">The configuration for accessing application settings.</param>
        /// <param name="jwtSecret">The secret key used for signing JWT tokens.</param>
        /// <remarks>
        /// The <paramref name="jwtSecret"/> should be a secure, randomly generated string.
        /// It is recommended to store it in a secure location, such as environment variables or a secrets manager.
        /// </remarks>
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

        /// <inheritdoc/>
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var response = new AuthResponseDto { Success = false, Errors = new List<string>() };

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
                response.Errors = result.Errors.Select(e => e.Description).ToList();
                return response;
            }

            response.Success = true;
            response.Token = GenerateJwtToken(user);
            response.User = MapToUserDto(user);

            return response;
        }

        /// <inheritdoc/>
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var response = new AuthResponseDto { Success = false, Errors = new List<string>() };

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                response.Errors.Add("Invalid email or password");
                return response;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                response.Errors.Add("Invalid email or password");
                return response;
            }

            // Update last active
            user.LastActive = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            response.Success = true;
            response.Token = GenerateJwtToken(user);
            response.User = MapToUserDto(user);

            return response;
        }

        /// <inheritdoc/>
        public string GenerateJwtToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Maps an ApplicationUser entity to a UserDto.
        /// </summary>
        /// <param name="user">The user to map.</param>
        /// <returns>A UserDto containing the user's public information.</returns>
        private UserDto MapToUserDto(ApplicationUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }
    }
}