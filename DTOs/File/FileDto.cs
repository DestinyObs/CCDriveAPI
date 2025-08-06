namespace TheDriveAPI.DTOs.File
{
    public class FileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public int? FolderId { get; set; }
        public string S3Key { get; set; } = string.Empty;
        public bool Trashed { get; set; }
        public int Version { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
    }
}
