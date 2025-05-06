using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReenbitTest.Core.DTOs;
using ReenbitTest.Core.Interfaces;

namespace ReenbitTest.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing user-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class
        /// </summary>
        /// <param name="userRepository">Repository for managing user-related operations</param>
        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Gets all users in the system
        /// </summary>
        /// <returns>
        /// 200 OK with collection of user DTOs
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userRepository.GetUsersAsync();

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName!,
                Email = u.Email!,
                FirstName = u.FirstName,
                LastName = u.LastName
            });

            return Ok(userDtos);
        }

        /// <summary>
        /// Gets a specific user by their ID
        /// </summary>
        /// <param name="id">The ID of the user to retrieve</param>
        /// <returns>
        /// 200 OK with user details
        /// 404 Not Found if user does not exist
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return Ok(userDto);
        }
    }
}