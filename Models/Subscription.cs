using System;

namespace CyberCloudDriveAPI.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int PlanId { get; set; }
        public Plan Plan { get; set; } = null!;
        public string Status { get; set; } = "active";
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}
