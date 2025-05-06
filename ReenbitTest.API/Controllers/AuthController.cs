using Microsoft.AspNetCore.Mvc;
using ReenbitTest.Core.DTOs;
using ReenbitTest.Core.Interfaces;

namespace ReenbitTest.API.Controllers
{
    /// <summary>
    /// Controller responsible for authentication operations such as user registration and login
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class
        /// </summary>
        /// <param name="authService">The authentication service implementation</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="registerDto">The registration information</param>
        /// <returns>
        /// 200 OK with authentication response on successful registration
        /// 400 Bad Request if registration fails
        /// </returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Authenticates a user and returns a token
        /// </summary>
        /// <param name="loginDto">The login credentials</param>
        /// <returns>
        /// 200 OK with authentication response on successful login
        /// 401 Unauthorized if login fails
        /// </returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }
    }
}