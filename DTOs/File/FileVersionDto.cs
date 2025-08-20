namespace TheDriveAPI.DTOs.File
{
    public class FileVersionDto
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public int Version { get; set; }
        public string S3Key { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
