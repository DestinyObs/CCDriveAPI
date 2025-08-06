using System;
using System.Collections.Generic;

namespace TheDriveAPI.Models
{
    public class File
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public int? FolderId { get; set; }
        public Folder? Folder { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public bool IsTrashed { get; set; }
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<FileVersion> Versions { get; set; } = new List<FileVersion>();
        public ICollection<SharedFile> SharedFiles { get; set; } = new List<SharedFile>();
        public string Permissions { get; set; } = "owner";
        public string S3Key { get; set; } = string.Empty;
    }
}
