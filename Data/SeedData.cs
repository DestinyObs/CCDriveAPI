using CyberCloudDriveAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CyberCloudDriveAPI.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(AppDbContext context, UserManager<User> userManager)
        {
            // Seed Plans
            if (!context.Plans.Any())
            {
                context.Plans.AddRange(
                    new Plan { Name = "Free", Price = 0, StorageLimit = 5_000_000_000, Features = "Basic features" },
                    new Plan { Name = "Pro", Price = 9.99M, StorageLimit = 100_000_000_000, Features = "Pro features" },
                    new Plan { Name = "Business", Price = 29.99M, StorageLimit = 1_000_000_000_000, Features = "Business features" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                var user = new User { UserName = "admin@cybercloud.com", Email = "admin@cybercloud.com", Name = "Admin", CreatedAt = DateTime.UtcNow, EmailConfirmed = true };
                await userManager.CreateAsync(user, "Admin123!");
            }

            // Seed Folders
            if (!context.Folders.Any())
            {
                context.Folders.Add(new Folder { Name = "Root", UserId = context.Users.First().Id, CreatedAt = DateTime.UtcNow });
                await context.SaveChangesAsync();
            }
        }
    }
}
