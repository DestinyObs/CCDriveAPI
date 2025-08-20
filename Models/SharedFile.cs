using System;

namespace TheDriveAPI.Models
{
    public class SharedFile
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public File File { get; set; } = null!;
        public string SharedWithUserId { get; set; } = string.Empty;
        public User SharedWithUser { get; set; } = null!;
        public string Permission { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
