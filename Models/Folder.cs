using System;
using System.Collections.Generic;

namespace TheDriveAPI.Models
{
    public class Folder
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public Folder? Parent { get; set; }
        public ICollection<Folder> Children { get; set; } = new List<Folder>();
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public ICollection<File> Files { get; set; } = new List<File>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
