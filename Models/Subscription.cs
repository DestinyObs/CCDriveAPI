using System;

namespace TheDriveAPI.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public int PlanId { get; set; }
        public Plan Plan { get; set; } = null!;
        public string Status { get; set; } = "active";
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? PaymentProvider { get; set; }
        public string? PaymentId { get; set; }
    }
}
