using TheDriveAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TheDriveAPI.Data
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
                    new Plan { Name = "Business", Price = 29.99M, StorageLimit = 1_000_000_000_000, Features = "Business features" },
                    new Plan { Name = "Enterprise", Price = 99.99M, StorageLimit = 10_000_000_000_000, Features = "Enterprise features" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                var admin = new User { UserName = "admin@thedrive.com", Email = "admin@thedrive.com", Name = "Admin", CreatedAt = DateTime.UtcNow, EmailConfirmed = true, Status = "active", IsVerified = true };
                var user1 = new User { UserName = "user1@thedrive.com", Email = "user1@thedrive.com", Name = "User One", CreatedAt = DateTime.UtcNow, EmailConfirmed = true, Status = "active", IsVerified = true };
                var user2 = new User { UserName = "user2@thedrive.com", Email = "user2@thedrive.com", Name = "User Two", CreatedAt = DateTime.UtcNow, EmailConfirmed = true, Status = "active", IsVerified = true };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.CreateAsync(user1, "User123!");
                await userManager.CreateAsync(user2, "User123!");
            }

            await context.SaveChangesAsync();
            var users = context.Users.ToList();
            var plans = context.Plans.ToList();

            // Seed Folders
            if (!context.Folders.Any())
            {
                context.Folders.AddRange(
                    new Folder { Name = "Root", UserId = users[0].Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Folder { Name = "Docs", UserId = users[1].Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Folder { Name = "Photos", UserId = users[2].Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            // Seed Subscriptions
            if (!context.Subscriptions.Any())
            {
                context.Subscriptions.AddRange(
                    new Subscription { UserId = users[0].Id, PlanId = plans[0].Id, Status = "active", StartedAt = DateTime.UtcNow },
                    new Subscription { UserId = users[1].Id, PlanId = plans[1].Id, Status = "active", StartedAt = DateTime.UtcNow },
                    new Subscription { UserId = users[2].Id, PlanId = plans[2].Id, Status = "active", StartedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            // Seed SupportTickets
            if (!context.SupportTickets.Any())
            {
                context.SupportTickets.AddRange(
                    new SupportTicket { UserId = users[0].Id, Subject = "Login Issue", Message = "Can't login to my account.", Priority = "high", Status = "open", CreatedAt = DateTime.UtcNow },
                    new SupportTicket { UserId = users[1].Id, Subject = "Upgrade Plan", Message = "How do I upgrade?", Priority = "medium", Status = "open", CreatedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            // Seed Files FIRST
            if (!context.Files.Any())
            {
                context.Files.AddRange(
                    new TheDriveAPI.Models.File { UserId = users[0].Id, Name = "file1.txt", Size = 1234, IsTrashed = false, Version = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, S3Key = "file1.txt" },
                    new TheDriveAPI.Models.File { UserId = users[1].Id, Name = "file2.jpg", Size = 5678, IsTrashed = false, Version = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, S3Key = "file2.jpg" }
                );
                await context.SaveChangesAsync();
            }

            var files = context.Files.ToList();

            // Seed FileVersions
            if (!context.FileVersions.Any() && files.Count > 0)
            {
                context.FileVersions.AddRange(
                    new FileVersion { FileId = files[0].Id, S3Key = "file1-v1", Version = 1, CreatedAt = DateTime.UtcNow },
                    new FileVersion { FileId = files[0].Id, S3Key = "file1-v2", Version = 2, CreatedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            // Seed SharedFiles
            if (!context.SharedFiles.Any() && files.Count > 0)
            {
                context.SharedFiles.Add(new SharedFile { FileId = files[0].Id, SharedWithUserId = users[1].Id, Permission = "read", CreatedAt = DateTime.UtcNow });
                await context.SaveChangesAsync();
            }

            // Seed Activities
            if (!context.Activities.Any())
            {
                context.Activities.AddRange(
                    new Activity { UserId = users[0].Id, Action = "login", Details = "User logged in", CreatedAt = DateTime.UtcNow },
                    new Activity { UserId = users[1].Id, Action = "upload", FileId = files.Count > 0 ? files[0].Id : (int?)null, Details = "Uploaded file", CreatedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            // Seed OTPs
            if (!context.OTPs.Any())
            {
                context.OTPs.AddRange(
                    new OTP { UserId = users[0].Id, Otp = "123456", ExpiresAt = DateTime.UtcNow.AddMinutes(10), Used = false },
                    new OTP { UserId = users[1].Id, Otp = "654321", ExpiresAt = DateTime.UtcNow.AddMinutes(10), Used = false }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
