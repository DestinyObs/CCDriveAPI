using CyberCloudDriveAPI.Services;
using Microsoft.AspNetCore.Mvc;
using CyberCloudDriveAPI.DTOs.Folder;
using Microsoft.AspNetCore.Authorization;

namespace CyberCloudDriveAPI.Controllers
{
    [ApiController]
    [Route("api/folders")]
    [Authorize]
    public class FolderController : ControllerBase
    {
        private readonly IFolderService _folderService;
        public FolderController(IFolderService folderService)
        {
            _folderService = folderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] dynamic body)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (body == null || body.name == null) return BadRequest(new { error = "Name required" });
            string name = body.name;
            int? parentId = body.parentId != null ? (int?)body.parentId : null;
            var folder = await _folderService.CreateFolderAsync(userId, name, parentId);
            return Ok(folder);
        }

        [HttpGet]
        public async Task<IActionResult> ListFolders([FromQuery] int? parentId)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var folders = await _folderService.ListFoldersAsync(userId, parentId);
            return Ok(folders);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _folderService.DeleteFolderAsync(userId, id);
            return Ok(new { success = result });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> RenameFolder(int id, [FromBody] dynamic body)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (body == null || body.name == null) return BadRequest(new { error = "Name required" });
            string name = body.name;
            var folder = await _folderService.RenameFolderAsync(userId, id, name);
            return Ok(folder);
        }
    }
}
