using System;
using System.Collections.Generic;

namespace CyberCloudDriveAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? PlanId { get; set; }
        public Plan? Plan { get; set; }
        public string Status { get; set; } = "active";
        public ICollection<File> Files { get; set; } = new List<File>();
        public ICollection<Folder> Folders { get; set; } = new List<Folder>();
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
