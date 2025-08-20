using Microsoft.AspNetCore.Http;

namespace TheDriveAPI.DTOs.File
{
    public class FileUploadDto
    {
        public required IFormFile File { get; set; }
        public int? FolderId { get; set; }
    }
}
