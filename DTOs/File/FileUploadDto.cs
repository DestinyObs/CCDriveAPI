using Microsoft.AspNetCore.Http;

namespace CyberCloudDriveAPI.DTOs.File
{
    public class FileUploadDto
    {
        public IFormFile File { get; set; }
        public int? FolderId { get; set; }
    }
}
