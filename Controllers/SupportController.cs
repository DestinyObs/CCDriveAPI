using Microsoft.AspNetCore.Mvc;

namespace CyberCloudDriveAPI.Controllers
{
    [ApiController]
    [Route("help")]
    public class SupportController : ControllerBase
    {
        [HttpGet("faq")]
        public IActionResult GetFaq() => Ok();

        [HttpPost("contact")]
        public IActionResult ContactSupport() => Ok();

        [HttpGet("resources")]
        public IActionResult GetResources() => Ok();
    }
}
