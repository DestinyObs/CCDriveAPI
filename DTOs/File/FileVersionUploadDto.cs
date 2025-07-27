using Microsoft.AspNetCore.Http;

namespace CyberCloudDriveAPI.DTOs.File
{
    public class FileVersionUploadDto
    {
        public IFormFile File { get; set; }
    }
}
