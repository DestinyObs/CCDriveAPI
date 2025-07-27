using Microsoft.AspNetCore.Mvc;

namespace CyberCloudDriveAPI.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        [HttpGet("profile")]
        public IActionResult GetProfile() => Ok();

        [HttpPatch("profile")]
        public IActionResult UpdateProfile() => Ok();

        [HttpPatch("password")]
        public IActionResult ChangePassword() => Ok();

        [HttpGet("usage")]
        public IActionResult GetUsage() => Ok();

        [HttpGet("activity")]
        public IActionResult GetActivity() => Ok();
    }
}
