using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevWebSecDemo.WebAPI.Controllers
{
    /// <summary>
    /// Testing controller for demo purposes
    /// </summary>
    [ApiController]
    [Route("api/test-token")]
    public class TestingController : Controller
    {
        public TestingController() { }

        /// <summary>
        /// Test if the token is valid.
        /// </summary>
        /// <returns>The OK if the Bearer token is valid, Unauthorized if Bearer is invalid.</returns>
        [Authorize]
        [HttpGet]
        public ActionResult TestToken(CancellationToken cancellationToken = default)
        {
            return new OkResult();
        }
    }
}