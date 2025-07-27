using CyberCloudDriveAPI.Services;
using Microsoft.AspNetCore.Mvc;
using CyberCloudDriveAPI.DTOs.Pricing;
using Microsoft.AspNetCore.Authorization;

namespace CyberCloudDriveAPI.Controllers
{
    [ApiController]
    [Route("api/pricing")]
    [Authorize]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;
        public PricingController(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        [HttpGet("plans")]
        public async Task<IActionResult> ListPlans()
        {
            var plans = await _pricingService.ListPlansAsync();
            return Ok(plans);
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] dynamic body)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (body == null) return BadRequest(new { error = "Plan name required" });
            var planNameProp = ((IDictionary<string, object>)body).ContainsKey("planName") ? body.planName : null;
            if (planNameProp == null) return BadRequest(new { error = "Plan name required" });
            string planName = planNameProp;
            var (success, redirectUrl) = await _pricingService.SubscribeAsync(userId, planName);
            return Ok(new { success, redirectUrl });
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _pricingService.CancelSubscriptionAsync(userId);
            return Ok(new { success = result });
        }
    }
}
