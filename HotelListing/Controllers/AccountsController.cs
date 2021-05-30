using AutoMapper;
using HotelListing.Data;
using HotelListing.Models;
using HotelListing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HotelListing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApiUser> _userManager;
        private readonly IAuthManager _authManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(UserManager<ApiUser> userManager, IAuthManager authManager, IMapper mapper, ILogger<AccountsController> logger)
        {
            _userManager = userManager;
            _authManager = authManager;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            _logger.LogInformation($"User registration attempt for {userDto.Email}");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<ApiUser>(userDto);
            user.UserName = userDto.Email;
            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                _logger.LogError($"User registration attempt failed for {userDto.Email}", ModelState);
                return BadRequest(ModelState);
            }

            await _userManager.AddToRolesAsync(user, userDto.Roles);

            _logger.LogInformation($"User registration successful for {userDto.Email}");
            return Accepted();
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
        {
            _logger.LogInformation($"Login attempt for {userDto.Email}");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _authManager.ValidateUserAsync(userDto))
            {
                _logger.LogInformation($"Login failed for {userDto.Email}. Invalid user name or password.");
                return Unauthorized();
            }

            _logger.LogInformation($"Login successful for {userDto.Email}");
            return Accepted(new { Token = await _authManager.CreateTokenAsync() });
        }
    }
}
