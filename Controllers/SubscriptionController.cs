using Microsoft.AspNetCore.Mvc;

namespace TheDriveAPI.Controllers
{
    [ApiController]
    [Route("pricing")]
    public class SubscriptionController : ControllerBase
    {
        [HttpGet("plans")]
        public IActionResult ListPlans() => Ok();

        [HttpPost("subscribe")]
        public IActionResult Subscribe() => Ok();

        [HttpPost("cancel")]
        public IActionResult CancelSubscription() => Ok();
    }
}
