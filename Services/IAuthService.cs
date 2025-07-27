using CyberCloudDriveAPI.DTOs.Auth;
using System.Threading.Tasks;

namespace CyberCloudDriveAPI.Services
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
    }
}
