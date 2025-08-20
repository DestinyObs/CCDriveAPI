using TheDriveAPI.Services;
using Microsoft.AspNetCore.Mvc;
using TheDriveAPI.DTOs.Activity;
using Microsoft.AspNetCore.Authorization;

namespace TheDriveAPI.Controllers
{
    [ApiController]
    [Route("api/user/activity")]
    [Authorize]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;
        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActivities([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var activities = await _activityService.GetActivitiesAsync(userId, page, pageSize);
            return Ok(activities);
        }
    }
}
