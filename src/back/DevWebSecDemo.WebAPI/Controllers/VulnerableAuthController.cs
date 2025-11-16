using DevWebSecDemo.Business;
using DevWebSecDemo.WebAPI.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace DevWebSecDemo.WebAPI.Controllers
{
    [ApiController]
    [Route("api/vulnerable")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class VulnerableAuthController : Controller
    {
        private readonly UserService _userService;
        private readonly TokenProvider _tokenProvider;
        private readonly ILogger<VulnerableAuthController> _logger;

        public VulnerableAuthController(
            UserService userService, 
            TokenProvider tokenProvider,
            ILogger<VulnerableAuthController> logger)
        {
            _userService = userService;
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        /// <summary>
        /// Register endpoint with weak password policy
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        public async Task<ActionResult> RegisterAsync([FromBody] UserIdentity userIdentity, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userIdentity.Username) || string.IsNullOrWhiteSpace(userIdentity.Password))
                {
                    return BadRequest("Username and password are required");
                }

                await _userService.CreateUserAsync(userIdentity.Username, userIdentity.Password, cancellationToken);
                
                return Ok(new { message = "User created successfully" });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        /// <summary>
        /// Login endpoint susceptible to brute force
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
        public async Task<ActionResult<TokenResponse>> LoginAsync([FromBody] UserIdentity userIdentity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _userService.IsCredentialsValidAsync(userIdentity.Username, userIdentity.Password, cancellationToken);

                var response = new TokenResponse
                {
                    Token = _tokenProvider.GenerateJwtToken(userIdentity.Username),
                    Expires = DateTime.UtcNow.AddDays(1),
                };

                _logger.LogInformation($"Successful login for user: {userIdentity.Username}");

                return Ok(response);
            }
            catch (ArgumentException e)
            {
                _logger.LogWarning($"Failed login attempt for user: {userIdentity.Username} - {e.Message}");
                
                return Unauthorized(new { message = e.Message });
            }
        }
    }
}
