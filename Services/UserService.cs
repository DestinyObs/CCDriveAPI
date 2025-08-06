using TheDriveAPI.DTOs.User;
using TheDriveAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TheDriveAPI.Data;

namespace TheDriveAPI.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public UserService(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<UserProfileDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.Users.Include(u => u.Plan).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new Exception("User not found");
            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email ?? "",
                PlanId = user.PlanId
            };
        }

        public async Task<UserProfileDto> UpdateProfileAsync(string userId, UserProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            user.Name = dto.Name;
            user.Email = dto.Email;
            await _userManager.UpdateAsync(user);
            return await GetProfileAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<object> GetStorageUsageAsync(string userId)
        {
            // Calculate storage usage from DB (sum file sizes)
            var used = await _context.Files.Where(f => f.UserId == userId).SumAsync(f => (long?)f.Size) ?? 0;
            var user = await _userManager.Users.Include(u => u.Plan).FirstOrDefaultAsync(u => u.Id == userId);
            var total = user?.Plan?.StorageLimit ?? 0;
            return new { storageUsed = used, totalStorage = total };
        }

        public async Task<object> GetActivityAsync(string userId, string? period)
        {
            // Filter activities by user and period
            var query = _context.Activities.Where(a => a.UserId == userId);
            if (!string.IsNullOrEmpty(period))
            {
                var now = DateTime.UtcNow;
                if (period == "today") query = query.Where(a => a.CreatedAt.Date == now.Date);
                else if (period == "week") query = query.Where(a => a.CreatedAt >= now.AddDays(-7));
                else if (period == "month") query = query.Where(a => a.CreatedAt >= now.AddMonths(-1));
            }
            var activities = await query.OrderByDescending(a => a.CreatedAt).Take(100).ToListAsync();
            return activities;
        }

        public async Task<bool> UpdatePreferencesAsync(string userId, DTOs.User.PreferencesDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            user.Preferences = System.Text.Json.JsonSerializer.Serialize(dto);
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<DTOs.User.ExportDto> ExportUserDataAsync(string userId)
        {
            // Gather user data
            var user = await _userManager.FindByIdAsync(userId);
            var files = await _context.Files.Where(f => f.UserId == userId).ToListAsync();
            var activities = await _context.Activities.Where(a => a.UserId == userId).ToListAsync();
            // TODO: Generate archive and upload to S3/Ceph, return URL
            // For now, return a placeholder
            return new DTOs.User.ExportDto { ArchiveUrl = "https://s3.example.com/user-archive.zip" };
        }

        public async Task<bool> DeleteAccountAsync(string userId, DTOs.User.DeleteAccountDto dto)
        {
            // Cascade delete user and all related data
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            // Delete files, folders, activities, shares, subscriptions, etc.
            _context.Files.RemoveRange(_context.Files.Where(f => f.UserId == userId));
            _context.Folders.RemoveRange(_context.Folders.Where(f => f.UserId == userId));
            _context.Activities.RemoveRange(_context.Activities.Where(a => a.UserId == userId));
            _context.Subscriptions.RemoveRange(_context.Subscriptions.Where(s => s.UserId == userId));
            _context.SharedFiles.RemoveRange(_context.SharedFiles.Where(sf => sf.SharedWithUserId == userId));
            _context.OTPs.RemoveRange(_context.OTPs.Where(o => o.UserId == userId));
            _context.SupportTickets.RemoveRange(_context.SupportTickets.Where(st => st.UserId == userId));
            await _context.SaveChangesAsync();
            await _userManager.DeleteAsync(user);
            return true;
        }

        public async Task<bool> UpgradePlanAsync(string userId, DTOs.User.UpgradeDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            user.PlanId = dto.PlanId;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<bool> Toggle2FAAsync(string userId, DTOs.User.PrivacyDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            // For demo, just store privacy settings as JSON
            user.NotificationSettings = System.Text.Json.JsonSerializer.Serialize(dto);
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UpdatePrivacyAsync(string userId, DTOs.User.PrivacyDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            user.NotificationSettings = System.Text.Json.JsonSerializer.Serialize(dto);
            await _userManager.UpdateAsync(user);
            return true;
        }
    }
}
