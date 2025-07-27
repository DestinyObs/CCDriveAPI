using Microsoft.AspNetCore.Http;

namespace CyberCloudDriveAPI.DTOs.File
{
    public class FileVersionUploadDto
    {
        public required IFormFile File { get; set; }
    }
}
