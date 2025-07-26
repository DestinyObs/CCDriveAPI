using System;

namespace CyberCloudDriveAPI.Models
{
    public class FileVersion
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public File File { get; set; } = null!;
        public string S3Key { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
