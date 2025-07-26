using Microsoft.EntityFrameworkCore;
using CyberCloudDriveAPI.Models;

namespace CyberCloudDriveAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<File> Files => Set<File>();
        public DbSet<Folder> Folders => Set<Folder>();
        public DbSet<FileVersion> FileVersions => Set<FileVersion>();
        public DbSet<SharedFile> SharedFiles => Set<SharedFile>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<OTP> OTPs => Set<OTP>();
        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Folder self-referencing
            modelBuilder.Entity<Folder>()
                .HasOne(f => f.Parent)
                .WithMany(f => f.Children)
                .HasForeignKey(f => f.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // User-File relationship
            modelBuilder.Entity<File>()
                .HasOne(f => f.User)
                .WithMany(u => u.Files)
                .HasForeignKey(f => f.UserId);

            // User-Folder relationship
            modelBuilder.Entity<Folder>()
                .HasOne(f => f.User)
                .WithMany(u => u.Folders)
                .HasForeignKey(f => f.UserId);

            // File-Folder relationship
            modelBuilder.Entity<File>()
                .HasOne(f => f.Folder)
                .WithMany(fol => fol.Files)
                .HasForeignKey(f => f.FolderId);

            // FileVersion-File relationship
            modelBuilder.Entity<FileVersion>()
                .HasOne(fv => fv.File)
                .WithMany(f => f.Versions)
                .HasForeignKey(fv => fv.FileId);

            // SharedFile relationships
            modelBuilder.Entity<SharedFile>()
                .HasOne(sf => sf.File)
                .WithMany(f => f.SharedFiles)
                .HasForeignKey(sf => sf.FileId);
            modelBuilder.Entity<SharedFile>()
                .HasOne(sf => sf.SharedWithUser)
                .WithMany()
                .HasForeignKey(sf => sf.SharedWithUserId);

            // Activity-User relationship
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(a => a.UserId);
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.File)
                .WithMany()
                .HasForeignKey(a => a.FileId);

            // Subscription relationships
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PlanId);

            // User-Plan relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Plan)
                .WithMany()
                .HasForeignKey(u => u.PlanId);

            // OTP relationships
            modelBuilder.Entity<OTP>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId);

            // SupportTicket-User relationship
            modelBuilder.Entity<SupportTicket>()
                .HasOne(st => st.User)
                .WithMany()
                .HasForeignKey(st => st.UserId);
        }
    }
}
