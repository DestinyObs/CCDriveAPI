using System;
using System.Collections.Generic;

namespace TheDriveAPI.Models
{
    using Microsoft.AspNetCore.Identity;
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? PlanId { get; set; }
        public Plan? Plan { get; set; }
        public string Status { get; set; } = "active";
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public DateTime JoinDate { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Preferences { get; set; }
        public string? NotificationSettings { get; set; }
        public ICollection<Folder> Folders { get; set; } = new List<Folder>();
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
