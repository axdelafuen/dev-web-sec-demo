using DevWebSecDemo.Business;
using DevWebSecDemo.WebAPI.Authentication;
using DevWebSecDemo.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevWebSecDemo.WebAPI.Controllers
{
    [ApiController]
    [Route("api/secure")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class SecureAuthController : Controller
    {
        private readonly UserService _userService;
        private readonly TokenProvider _tokenProvider;
        private readonly ILogger<SecureAuthController> _logger;
        private readonly RateLimitingService _rateLimitingService;
        private readonly AccountLockoutService _accountLockoutService;

        public SecureAuthController(
            UserService userService,
            TokenProvider tokenProvider,
            ILogger<SecureAuthController> logger,
            RateLimitingService rateLimitingService,
            AccountLockoutService accountLockoutService)
        {
            _userService = userService;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _rateLimitingService = rateLimitingService;
            _accountLockoutService = accountLockoutService;
        }

        /// <summary>
        /// Register endpoint with strong password policy
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        public async Task<ActionResult> RegisterAsync([FromBody] UserIdentity registration, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!IsPasswordStrong(registration.Password))
                {
                    return BadRequest(new { message = "Password must be at least 8 characters with uppercase, lowercase, number, and special character" });
                }

                await _userService.CreateUserAsync(registration.Username, registration.Password, cancellationToken);

                _logger.LogInformation("New user registration successful");
                return Ok(new { message = "Registration successful" });
            }
            catch (Exception)
            {
                _logger.LogWarning("Registration attempt failed");
                return BadRequest(new { message = "Registration failed. Please try again." });
            }
        }

        /// <summary>
        /// Login endpoint with multiple security layers
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
        public async Task<ActionResult<TokenResponse>> LoginAsync([FromBody] UserIdentity loginRequest, CancellationToken cancellationToken = default)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (_rateLimitingService.IsRateLimited(clientIp))
            {
                _logger.LogWarning($"Rate limit exceeded for IP: {clientIp}");
                return StatusCode(429, new { message = "Too many attempts. Please try again later." });
            }

            if (_accountLockoutService.IsLockedOut(loginRequest.Username))
            {
                var lockoutTime = _accountLockoutService.GetLockoutTimeRemaining(loginRequest.Username);
                _logger.LogWarning($"Login attempt on locked account");
                return Unauthorized(new { message = $"Account temporarily locked. Try again in {lockoutTime.TotalMinutes:F0} minutes." });
            }

            try
            {
                await _userService.IsCredentialsValidAsync(loginRequest.Username, loginRequest.Password, cancellationToken);

                _accountLockoutService.ResetFailedAttempts(loginRequest.Username);

                var response = new TokenResponse
                {
                    Token = _tokenProvider.GenerateJwtToken(loginRequest.Username),
                    Expires = DateTime.UtcNow.AddDays(1),
                };

                _logger.LogInformation($"Successful authentication from IP: {clientIp}");

                return Ok(response);
            }
            catch (ArgumentException)
            {
                _accountLockoutService.RecordFailedAttempt(loginRequest.Username);
                _rateLimitingService.RecordAttempt(clientIp);

                var remainingAttempts = _accountLockoutService.GetRemainingAttempts(loginRequest.Username);

                _logger.LogWarning($"Failed authentication attempt from IP: {clientIp}");

                if (remainingAttempts > 0)
                {
                    return Unauthorized(new { message = $"Invalid credentials. {remainingAttempts} attempts remaining." });
                }
                else
                {
                    return Unauthorized(new { message = "Account locked due to multiple failed attempts." });
                }
            }
        }

        private static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
    }
}
