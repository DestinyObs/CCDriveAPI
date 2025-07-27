
using CyberCloudDriveAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CyberCloudDriveAPI.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> ListFiles([FromQuery] int? folderId)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var files = await _fileService.ListFilesAsync(userId, folderId);
            return Ok(files);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] int? folderId)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (file == null || file.Length == 0) return BadRequest(new { error = "No file uploaded" });
            if (file.Length > 5_000_000_000) return BadRequest(new { error = "File too large (max 5GB)" });
            var result = await _fileService.UploadFileAsync(userId, file, folderId);
            return Ok(result);
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var file = await _fileService.DownloadFileAsync(userId, id);
            // For now, return download URL; in production, stream file
            return Ok(file);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _fileService.DeleteFileAsync(userId, id);
            return Ok(new { success = result });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> RenameFile(int id, [FromBody] dynamic body)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (body == null) return BadRequest(new { error = "Name required" });
            var nameProp = ((IDictionary<string, object>)body).ContainsKey("name") ? body.name : null;
            if (nameProp == null || string.IsNullOrEmpty(nameProp)) return BadRequest(new { error = "Name required" });
            string name = nameProp;
            var file = await _fileService.RenameFileAsync(userId, id, name);
            return Ok(file);
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> MoveFile(int id, [FromBody] dynamic body)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (body == null) return BadRequest(new { error = "FolderId required" });
            var folderIdProp = ((IDictionary<string, object>)body).ContainsKey("folderId") ? body.folderId : null;
            if (folderIdProp == null) return BadRequest(new { error = "FolderId required" });
            int folderId = folderIdProp;
            var file = await _fileService.MoveFileAsync(userId, id, folderId);
            return Ok(file);
        }

        [HttpPost("{id}/share")]
        public async Task<IActionResult> ShareFile(int id, [FromBody] dynamic body)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (body == null) return BadRequest(new { error = "Email and permission required" });
            var emailProp = ((IDictionary<string, object>)body).ContainsKey("email") ? body.email : null;
            var permissionProp = ((IDictionary<string, object>)body).ContainsKey("permission") ? body.permission : null;
            if (emailProp == null || permissionProp == null)
                return BadRequest(new { error = "Email and permission required" });
            string email = emailProp;
            string permission = permissionProp;
            var result = await _fileService.ShareFileAsync(userId, id, email, permission);
            return Ok(new { success = result });
        }

        [HttpGet("shared")]
        public async Task<IActionResult> ListSharedFiles()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var files = await _fileService.ListSharedFilesAsync(userId);
            return Ok(files);
        }

        [HttpGet("trash")]
        public async Task<IActionResult> ListTrashedFiles()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var files = await _fileService.ListTrashedFilesAsync(userId);
            return Ok(files);
        }

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> RestoreFile(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var file = await _fileService.RestoreFileAsync(userId, id);
            return Ok(file);
        }

        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetFileVersions(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var versions = await _fileService.GetFileVersionsAsync(userId, id);
            return Ok(versions);
        }

        [HttpDelete("{id}/permanent")]
        public async Task<IActionResult> PermanentDeleteFile(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _fileService.PermanentDeleteFileAsync(userId, id);
            return Ok(new { success = result });
        }

        [HttpPost("{id}/versions")]
        public async Task<IActionResult> UploadFileVersion(int id, [FromForm] IFormFile file)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (file == null || file.Length == 0) return BadRequest(new { error = "No file uploaded" });
            var result = await _fileService.UploadFileVersionAsync(userId, id, file);
            return Ok(result);
        }

        [HttpPatch("{id}/versions/{versionId}/restore")]
        public async Task<IActionResult> RestoreFileVersion(int id, int versionId)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var file = await _fileService.RestoreFileVersionAsync(userId, id, versionId);
            return Ok(file);
        }

        [HttpDelete("{id}/versions/{versionId}")]
        public async Task<IActionResult> DeleteFileVersion(int id, int versionId)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _fileService.DeleteFileVersionAsync(userId, id, versionId);
            return Ok(new { success = result });
        }

        [HttpGet("{id}/preview")]
        public async Task<IActionResult> PreviewFile(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var preview = await _fileService.PreviewFileAsync(userId, id);
            return Ok(preview);
        }

        [HttpPost("{id}/copy")]
        public async Task<IActionResult> CopyFile(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var file = await _fileService.CopyFileAsync(userId, id);
            return Ok(file);
        }

        [HttpPost("{id}/favorite")]
        public async Task<IActionResult> FavoriteFile(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _fileService.FavoriteFileAsync(userId, id);
            return Ok(new { success = result });
        }
    }
}
