using Microsoft.AspNetCore.Mvc;
using TheDriveAPI.DTOs.Auth;
using TheDriveAPI.Services;

namespace TheDriveAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] DTOs.Auth.OtpDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email))
                return BadRequest(new { code = "VALIDATION_ERROR", error = "Email is required." });
            try
            {
                var result = await _authService.ResendOtpAsync(dto.Email);
                if (!result)
                    return NotFound(new { code = "USER_NOT_FOUND", error = "User not found." });
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = "RESEND_OTP_ERROR", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password) || string.IsNullOrEmpty(dto.Name))
                return BadRequest(new { code = "VALIDATION_ERROR", error = "Email, password, and name are required." });
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = "REGISTER_ERROR", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return BadRequest(new { code = "VALIDATION_ERROR", error = "Email and password are required." });
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { code = "AUTH_ERROR", error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = "LOGIN_ERROR", error = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] DTOs.Auth.OtpDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Otp))
                return BadRequest(new { code = "VALIDATION_ERROR", error = "Email and OTP are required." });
            try
            {
                var result = await _authService.VerifyOtpAsync(dto);
                if (!result)
                    return Unauthorized(new { code = "OTP_INVALID", error = "Invalid or expired OTP." });
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = "OTP_ERROR", error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] DTOs.Auth.ForgotPasswordDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email))
                return BadRequest(new { code = "VALIDATION_ERROR", error = "Email is required." });
            try
            {
                var result = await _authService.ForgotPasswordAsync(dto);
                if (!result)
                    return NotFound(new { code = "USER_NOT_FOUND", error = "User not found." });
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = "FORGOT_PASSWORD_ERROR", error = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] DTOs.Auth.ResetPasswordDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Token) || string.IsNullOrEmpty(dto.Password))
                return BadRequest(new { code = "VALIDATION_ERROR", error = "Email, token, and new password are required." });
            try
            {
                var result = await _authService.ResetPasswordAsync(dto);
                if (!result)
                    return BadRequest(new { code = "RESET_FAILED", error = "Password reset failed." });
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = "RESET_ERROR", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { code = "AUTH_REQUIRED", error = "User not authenticated." });
            try
            {
                var result = await _authService.LogoutAsync(userId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = "LOGOUT_ERROR", error = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] DTOs.Auth.RefreshDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.RefreshToken))
                return BadRequest(new { code = "VALIDATION_ERROR", error = "Refresh token is required." });
            try
            {
                var result = await _authService.RefreshAsync(dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { code = "REFRESH_ERROR", error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = "REFRESH_ERROR", error = ex.Message });
            }
        }
    }
}
