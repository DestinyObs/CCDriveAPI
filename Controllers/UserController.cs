using Microsoft.AspNetCore.Mvc;
using TheDriveAPI.DTOs.User;
using TheDriveAPI.DTOs.Auth;
using TheDriveAPI.Services;

namespace TheDriveAPI.Controllers
{
    [ApiController]
    [Route("api/user")]
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
        public async Task<IActionResult> GetUsage()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var usage = await _userService.GetStorageUsageAsync(userId);
            return Ok(usage);
        }

        // [HttpGet("activity")]
        // public async Task<IActionResult> GetActivity([FromQuery] string? period)
        // {
        //     var userId = User.FindFirst("sub")?.Value;
        //     if (string.IsNullOrEmpty(userId)) return Unauthorized();
        //     var activity = await _userService.GetActivityAsync(userId, period);
        //     return Ok(activity);
        // }

        [HttpPatch("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] PreferencesDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _userService.UpdatePreferencesAsync(userId, dto);
            return Ok(new { success = result });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportUserData()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var export = await _userService.ExportUserDataAsync(userId);
            return Ok(export);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _userService.DeleteAccountAsync(userId, dto);
            return Ok(new { success = result });
        }

        [HttpPatch("upgrade")]
        public async Task<IActionResult> UpgradePlan([FromBody] UpgradeDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _userService.UpgradePlanAsync(userId, dto);
            return Ok(new { success = result });
        }

        [HttpPatch("2fa")]
        public async Task<IActionResult> Toggle2FA([FromBody] PrivacyDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _userService.Toggle2FAAsync(userId, dto);
            return Ok(new { success = result });
        }

        [HttpPatch("privacy")]
        public async Task<IActionResult> UpdatePrivacy([FromBody] PrivacyDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _userService.UpdatePrivacyAsync(userId, dto);
            return Ok(new { success = result });
        }
    }
}
