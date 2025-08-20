using TheDriveAPI.Services;
using TheDriveAPI.DTOs.Folder;
using Microsoft.AspNetCore.Mvc;
using TheDriveAPI.DTOs.Folder;
using Microsoft.AspNetCore.Authorization;

namespace TheDriveAPI.Controllers
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
        public async Task<IActionResult> CreateFolder([FromBody] FolderCreateDto body)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (body == null || string.IsNullOrEmpty(body.Name)) return BadRequest(new { error = "Name required" });
            var folder = await _folderService.CreateFolderAsync(userId, body.Name, body.ParentId);
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
        public async Task<IActionResult> RenameFolder(int id, [FromBody] FolderRenameDto body)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (body == null || string.IsNullOrEmpty(body.Name)) return BadRequest(new { error = "Name required" });
            var folder = await _folderService.RenameFolderAsync(userId, id, body.Name);
            return Ok(folder);
        }
    }
}
