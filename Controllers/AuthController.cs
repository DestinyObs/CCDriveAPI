using Microsoft.AspNetCore.Mvc;

namespace CyberCloudDriveAPI.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login() => Ok();

        [HttpPost("register")]
        public IActionResult Register() => Ok();

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp() => Ok();

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword() => Ok();

        [HttpPost("reset-password")]
        public IActionResult ResetPassword() => Ok();

        [HttpPost("logout")]
        public IActionResult Logout() => Ok();

        [HttpPost("refresh")]
        public IActionResult Refresh() => Ok();
    }
}
