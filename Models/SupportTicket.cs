using System;

namespace TheDriveAPI.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = "open";
        public DateTime CreatedAt { get; set; }
    }
}
