using Microsoft.AspNetCore.Mvc;
using CyberCloudDriveAPI.DTOs.User;
using CyberCloudDriveAPI.DTOs.Auth;
using CyberCloudDriveAPI.Services;

namespace CyberCloudDriveAPI.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // In real use, get userId from JWT claims
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            try
            {
                var profile = await _userService.GetProfileAsync(userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] DTOs.User.UserProfileDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            try
            {
                var profile = await _userService.UpdateProfileAsync(userId, dto);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("password")]
        public async Task<IActionResult> ChangePassword([FromBody] DTOs.Auth.ChangePasswordDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            try
            {
                var result = await _userService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("usage")]
        public IActionResult GetUsage() => Ok();

        [HttpGet("activity")]
        public IActionResult GetActivity() => Ok();
    }
}
