using Microsoft.AspNetCore.Mvc;

namespace CyberCloudDriveAPI.Controllers
{
    [ApiController]
    [Route("folders")]
    public class FolderController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateFolder() => Ok();

        [HttpGet]
        public IActionResult ListFolders() => Ok();

        [HttpDelete("{id}")]
        public IActionResult DeleteFolder(int id) => Ok();

        [HttpPatch("{id}")]
        public IActionResult RenameFolder(int id) => Ok();
    }
}
