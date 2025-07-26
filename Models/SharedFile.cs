using System;

namespace CyberCloudDriveAPI.Models
{
    public class SharedFile
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public File File { get; set; } = null!;
        public int SharedWithUserId { get; set; }
        public User SharedWithUser { get; set; } = null!;
        public string Permission { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
