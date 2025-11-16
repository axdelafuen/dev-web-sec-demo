using DevWebSecDemo.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevWebSecDemo.WebAPI.Controllers
{
    /// <summary>
    /// Admin controller for demo purposes - reset security mechanisms
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    public class AdminController : Controller
    {
        private readonly RateLimitingService _rateLimitingService;
        private readonly AccountLockoutService _accountLockoutService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            RateLimitingService rateLimitingService,
            AccountLockoutService accountLockoutService,
            ILogger<AdminController> logger)
        {
            _rateLimitingService = rateLimitingService;
            _accountLockoutService = accountLockoutService;
            _logger = logger;
        }

        /// <summary>
        /// Reset all rate limiting and account lockout counters
        /// For demo purposes only
        /// </summary>
        [HttpPost("reset")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        public ActionResult Reset()
        {
            _rateLimitingService.ResetAll();
            _accountLockoutService.ResetAll();
            
            _logger.LogInformation("Security mechanisms reset - all rate limits and lockouts cleared");
            
            return Ok(new { message = "All rate limits and account lockouts have been reset" });
        }

        /// <summary>
        /// Reset rate limiting for a specific IP
        /// </summary>
        [HttpPost("reset-rate-limit/{identifier}")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        public ActionResult ResetRateLimit(string identifier)
        {
            _rateLimitingService.Reset(identifier);
            
            _logger.LogInformation($"Rate limit reset for: {identifier}");
            
            return Ok(new { message = $"Rate limit reset for {identifier}" });
        }

        /// <summary>
        /// Reset account lockout for a specific username
        /// </summary>
        [HttpPost("reset-lockout/{username}")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        public ActionResult ResetLockout(string username)
        {
            _accountLockoutService.ResetFailedAttempts(username);
            
            _logger.LogInformation($"Account lockout reset for user");
            
            return Ok(new { message = $"Account lockout reset for {username}" });
        }

        /// <summary>
        /// Get lockout status for demonstration purposes
        /// </summary>
        [HttpGet("lockout-status/{username}")]
        public ActionResult GetLockoutStatus(string username)
        {
            var isLocked = _accountLockoutService.IsLockedOut(username);
            var remainingAttempts = _accountLockoutService.GetRemainingAttempts(username);
            var lockoutTime = _accountLockoutService.GetLockoutTimeRemaining(username);

            return Ok(new
            {
                isLocked,
                remainingAttempts,
                lockoutMinutesRemaining = lockoutTime.TotalMinutes
            });
        }

        /// <summary>
        /// Get IP rate limit status for demonstration purposes
        /// </summary>
        [HttpGet("ip-status")]
        public ActionResult GetIpStatus()
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var isRateLimited = _rateLimitingService.IsRateLimited(clientIp);
            var remainingAttempts = _rateLimitingService.GetRemainingAttempts(clientIp);
            var timeRemaining = _rateLimitingService.GetTimeRemaining(clientIp);

            return Ok(new
            {
                clientIp,
                isRateLimited,
                remainingAttempts,
                timeRemainingMinutes = timeRemaining.TotalMinutes
            });
        }
    }
}
