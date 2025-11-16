using DevWebSecDemo.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevWebSecDemo.WebAPI.Controllers
{
    /// <summary>
    /// Status controller for demo purposes - get statuses
    /// </summary>
    [ApiController]
    [Route("api/status")]
    public class StatusController : Controller
    {
        private readonly RateLimitingService _rateLimitingService;
        private readonly AccountLockoutService _accountLockoutService;

        public StatusController(
            RateLimitingService rateLimitingService,
            AccountLockoutService accountLockoutService)
        {
            _rateLimitingService = rateLimitingService;
            _accountLockoutService = accountLockoutService;
        }

        /// <summary>
        /// Get lockout status for demonstration purposes
        /// </summary>
        [HttpGet("lockout-user/{username}")]
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
        [HttpGet("lockout-ip")]
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