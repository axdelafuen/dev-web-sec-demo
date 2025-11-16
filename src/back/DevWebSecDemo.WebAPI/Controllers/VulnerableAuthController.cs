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
        /// VULNERABLE: Register endpoint with weak password policy
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        public async Task<ActionResult> RegisterAsync([FromBody] UserIdentity userIdentity, CancellationToken cancellationToken = default)
        {
            try
            {
                // VULNERABILITY: No password strength validation
                if (string.IsNullOrWhiteSpace(userIdentity.Username) || string.IsNullOrWhiteSpace(userIdentity.Password))
                {
                    return BadRequest("Username and password are required");
                }

                // VULNERABILITY: Allows weak passwords like "123"
                await _userService.CreateUserAsync(userIdentity.Username, userIdentity.Password, cancellationToken);
                
                return Ok(new { message = "User created successfully" });
            }
            catch (Exception e)
            {
                // VULNERABILITY: Detailed error messages
                return BadRequest(new { message = e.Message });
            }
        }

        /// <summary>
        /// VULNERABLE: Login endpoint susceptible to brute force
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
        public async Task<ActionResult<TokenResponse>> LoginAsync([FromBody] UserIdentity userIdentity, CancellationToken cancellationToken = default)
        {
            try
            {
                // VULNERABILITY: No rate limiting or attempt tracking
                await _userService.IsCredentialsValidAsync(userIdentity.Username, userIdentity.Password, cancellationToken);

                var response = new TokenResponse
                {
                    Token = _tokenProvider.GenerateJwtToken(userIdentity.Username),
                    Expires = DateTime.UtcNow.AddDays(1),
                };

                // Log success with detailed info (visible in demo)
                _logger.LogInformation($"Successful login for user: {userIdentity.Username}");

                return Ok(response);
            }
            catch (ArgumentException e)
            {
                // VULNERABILITY: Detailed error messages reveal if username exists
                // This allows attackers to enumerate valid usernames
                _logger.LogWarning($"Failed login attempt for user: {userIdentity.Username} - {e.Message}");
                
                return Unauthorized(new { message = e.Message });
            }
        }

        /// <summary>
        /// VULNERABLE: Allows unlimited password reset requests
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        public ActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // VULNERABILITY: No rate limiting, allows email enumeration
            _logger.LogInformation($"Password reset requested for: {request.Username}");
            
            // VULNERABILITY: Different responses based on username existence
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new { message = "Username is required" });
            }

            // Simulate checking if user exists (reveals information)
            return Ok(new { message = $"If user '{request.Username}' exists, a reset link will be sent" });
        }
    }
}
