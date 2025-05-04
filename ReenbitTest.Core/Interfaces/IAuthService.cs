using ReenbitTest.Core.DTOs;
using ReenbitTest.Core.Entities;

namespace ReenbitTest.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        string GenerateJwtToken(ApplicationUser user);
    }
}