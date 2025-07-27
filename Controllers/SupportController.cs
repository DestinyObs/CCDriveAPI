using CyberCloudDriveAPI.Services;
using CCDriveAPI.DTOs.Support;
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
        public async Task<IActionResult> Contact([FromBody] SupportContactDto body)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (body == null || string.IsNullOrEmpty(body.Subject) || string.IsNullOrEmpty(body.Message) || string.IsNullOrEmpty(body.Priority)) return BadRequest(new { error = "Subject, message, priority required" });
            // ...existing code...
            var result = await _supportService.CreateTicketAsync(userId, body.Subject, body.Message, body.Priority);
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
