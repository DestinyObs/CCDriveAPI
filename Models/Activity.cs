using System;

namespace TheDriveAPI.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public string Action { get; set; } = string.Empty;
        public int? FileId { get; set; }
        public File? File { get; set; }
        public string Details { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
