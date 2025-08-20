using TheDriveAPI.DTOs.Auth;
using System.Threading.Tasks;

namespace TheDriveAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<bool> VerifyOtpAsync(OtpDto dto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
        Task<bool> LogoutAsync(string userId);
        Task<AuthResponseDto> RefreshAsync(RefreshDto dto);
        Task<bool> ResendOtpAsync(string email);
    }
}
