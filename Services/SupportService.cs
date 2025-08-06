using TheDriveAPI.Models;
using TheDriveAPI.DTOs.Support;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using TheDriveAPI.Data;

namespace TheDriveAPI.Services
{
    public interface ISupportService
    {
        Task<IEnumerable<SupportTicketDto>> ListTicketsAsync(string userId);
        Task<bool> CreateTicketAsync(string userId, string subject, string message, string priority);
    }

    public class SupportService : ISupportService
    {
        private readonly AppDbContext _context;
        public SupportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SupportTicketDto>> ListTicketsAsync(string userId)
        {
            var tickets = await _context.SupportTickets.AsNoTracking().Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt).ToListAsync();
            return tickets.Select(t => new SupportTicketDto
            {
                Id = t.Id,
                UserId = t.UserId,
                Subject = t.Subject,
                Message = t.Message,
                Priority = t.Priority,
                Status = t.Status,
                CreatedAt = t.CreatedAt.ToString("o")
            });
        }

        public async Task<bool> CreateTicketAsync(string userId, string subject, string message, string priority)
        {
            var ticket = new SupportTicket
            {
                UserId = userId,
                Subject = subject,
                Message = message,
                Priority = priority,
                Status = "open",
                CreatedAt = DateTime.UtcNow
            };
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();
            // TODO: Optionally notify support team
            return true;
        }
    }
}
