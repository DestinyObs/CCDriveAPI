using Microsoft.AspNetCore.Mvc;

namespace CyberCloudDriveAPI.Controllers
{
    [ApiController]
    [Route("files")]
    public class FileController : ControllerBase
    {
        [HttpGet]
        public IActionResult ListFiles() => Ok();

        [HttpPost("upload")]
        public IActionResult UploadFile() => Ok();

        [HttpGet("{id}/download")]
        public IActionResult DownloadFile(int id) => Ok();

        [HttpDelete("{id}")]
        public IActionResult DeleteFile(int id) => Ok();

        [HttpPatch("{id}")]
        public IActionResult RenameFile(int id) => Ok();

        [HttpPatch("{id}/move")]
        public IActionResult MoveFile(int id) => Ok();

        [HttpPost("{id}/share")]
        public IActionResult ShareFile(int id) => Ok();

        [HttpGet("shared")]
        public IActionResult ListSharedFiles() => Ok();

        [HttpGet("trash")]
        public IActionResult ListTrashedFiles() => Ok();

        [HttpPatch("{id}/restore")]
        public IActionResult RestoreFile(int id) => Ok();

        [HttpGet("{id}/versions")]
        public IActionResult GetFileVersions(int id) => Ok();
    }
}
