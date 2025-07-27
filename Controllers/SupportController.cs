using CyberCloudDriveAPI.Services;
using Microsoft.AspNetCore.Mvc;
using CyberCloudDriveAPI.DTOs.Support;
using Microsoft.AspNetCore.Authorization;

namespace CyberCloudDriveAPI.Controllers
{
    [ApiController]
    [Route("api/help")]
    [Authorize]
    public class SupportController : ControllerBase
    {
        private readonly ISupportService _supportService;
        public SupportController(ISupportService supportService)
        {
            _supportService = supportService;
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> ListTickets()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var tickets = await _supportService.ListTicketsAsync(userId);
            return Ok(tickets);
        }

        [HttpPost("contact")]
        public async Task<IActionResult> Contact([FromBody] dynamic body)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (body == null) return BadRequest(new { error = "Subject, message, priority required" });
            var subjectProp = ((IDictionary<string, object>)body).ContainsKey("subject") ? body.subject : null;
            var messageProp = ((IDictionary<string, object>)body).ContainsKey("message") ? body.message : null;
            var priorityProp = ((IDictionary<string, object>)body).ContainsKey("priority") ? body.priority : null;
            if (subjectProp == null || messageProp == null || priorityProp == null)
                return BadRequest(new { error = "Subject, message, priority required" });
            string subject = subjectProp;
            string message = messageProp;
            string priority = priorityProp;
            var result = await _supportService.CreateTicketAsync(userId, subject, message, priority);
            return Ok(new { success = result });
        }

        [HttpGet("faq")]
        public IActionResult GetFaq()
        {
            // TODO: Query FAQ from DB or static file
            var faq = new[] {
                new { category = "General", questions = new[] { new { q = "What is CyberCloud Drive?", a = "A secure cloud storage platform." } } }
            };
            return Ok(faq);
        }

        [HttpGet("resources")]
        public IActionResult GetResources()
        {
            // TODO: Query resources from DB or static file
            var resources = new[] { new { resource = "https://docs.cybercloud.com" } };
            return Ok(resources);
        }
    }
}
