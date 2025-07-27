using Microsoft.AspNetCore.Mvc;

namespace CyberCloudDriveAPI.Controllers
{
    [ApiController]
    [Route("activity")]
    public class ActivityController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetActivities() => Ok();
    }
}
