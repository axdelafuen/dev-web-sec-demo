using DevWebSecDemo.Business;
using Microsoft.AspNetCore.Mvc;

namespace DevWebSecDemo.WebAPI.Controllers
{
    /// <summary>
    /// Database controller for demo purposes - manage database
    /// </summary>
    [ApiController]
    [Route("api/database")]
    public class DatabaseManagerController : Controller
    {
        private readonly UserService _userService;

        public DatabaseManagerController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Delete all users in database
        /// </summary>
        [HttpDelete("delete-users")]
        public async Task<ActionResult> DeleteAllUsersAsync(CancellationToken cancellationToken = default)
        {
            await _userService.DaleteAllUsersAsync(cancellationToken);

            return Ok();
        }

        /// <summary>
        /// List all users in database
        /// </summary>
        [HttpGet("list-users")]
        public async Task<ActionResult> ListAllUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userService.ListAllUsersAsync(cancellationToken);

            return Ok(users);
        }
    }
}