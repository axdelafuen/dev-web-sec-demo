using DevWebSecDemo.Business;
using DevWebSecDemo.WebAPI.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace DevWebSecDemo.WebAPI.Controllers
{
    [ApiController]
    [Route("user")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class UserController : Controller
    {
        private readonly UserService _userService;

        private readonly TokenProvider _tokenProvider;
        
        public UserController(UserService userService, TokenProvider tokenProvider)
        {
            _userService = userService;
            _tokenProvider = tokenProvider;
        }

        /// <summary>
        /// Create a new user with login/password.
        /// </summary>
        /// <param name="userIdentity">The credentials (username/password).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost("register")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateUserAsync([FromBody] UserIdentity userIdentity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _userService.CreateUserAsync(userIdentity.Username, userIdentity.Password, cancellationToken);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }

            return new OkResult();
        }

        /// <summary>
        /// Get a Jwt token if the credentials are registered.
        /// </summary>
        /// <param name="userIdentity">The credentials (username/password).</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The Bearer token and it's expiration datetime (UTC)</returns>
        [HttpPost("login")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> GetTokenAsync([FromBody] UserIdentity userIdentity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _userService.IsCredentialsValidAsync(userIdentity.Username, userIdentity.Password, cancellationToken);
            }
            catch (ArgumentException e)
            {
                return new BadRequestObjectResult(e.Message);
            }

            var response = new TokenResponse
            {
                Token = _tokenProvider.GenerateJwtToken(userIdentity.Username),
                Expires = DateTime.UtcNow.AddDays(1),
            };

            return new OkObjectResult(response);
        }
    }
}
