using Microsoft.AspNetCore.Http;

namespace TheDriveAPI.DTOs.File
{
    public class FileVersionUploadDto
    {
        public required IFormFile File { get; set; }
    }
}
