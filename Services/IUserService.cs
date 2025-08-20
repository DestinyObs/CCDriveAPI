using TheDriveAPI.DTOs.User;
using System.Threading.Tasks;

namespace TheDriveAPI.Services
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(string userId);
        Task<UserProfileDto> UpdateProfileAsync(string userId, UserProfileDto dto);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<object> GetStorageUsageAsync(string userId);
        Task<object> GetActivityAsync(string userId, string? period);
        Task<bool> UpdatePreferencesAsync(string userId, DTOs.User.PreferencesDto dto);
        Task<DTOs.User.ExportDto> ExportUserDataAsync(string userId);
        Task<bool> DeleteAccountAsync(string userId, DTOs.User.DeleteAccountDto dto);
        Task<bool> UpgradePlanAsync(string userId, DTOs.User.UpgradeDto dto);
        Task<bool> Toggle2FAAsync(string userId, DTOs.User.PrivacyDto dto);
        Task<bool> UpdatePrivacyAsync(string userId, DTOs.User.PrivacyDto dto);
    }
}
