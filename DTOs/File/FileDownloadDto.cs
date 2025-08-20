namespace TheDriveAPI.DTOs.File
{
    public class FileDownloadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
